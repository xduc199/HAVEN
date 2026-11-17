using System.Net;
using System.Net.Http.Headers;
using Haven.Domain.Properties;
using Haven.Infrastructure.Context;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Haven.IntegrationTests;

public sealed class ListingImagesMvcTests
{
    [Fact]
    public async Task OwnerCanUploadSetPrimaryDeleteAndReadDraftImagesButPublicCannot()
    {
        var storage = Path.Combine(Path.GetTempPath(), "haven-image-tests", Guid.NewGuid().ToString("N"));
        using var factory = CreateFactory(storage);
        try
        {
            using var owner = AntiforgeryTestClient.Create(factory);
            var listingId = await CreateListingAsync(factory, owner, "images.owner@example.com");
            using var upload = await UploadAsync(owner, listingId, "first.jpg", Jpeg);
            Assert.Equal(HttpStatusCode.Redirect, upload.StatusCode);
            var first = await GetImagesAsync(factory, listingId);
            Assert.Single(first);
            Assert.True(first[0].IsPrimary);

            using var secondUpload = await UploadAsync(owner, listingId, "second.png", Png, "image/png");
            Assert.Equal(HttpStatusCode.Redirect, secondUpload.StatusCode);
            var images = await GetImagesAsync(factory, listingId);
            Assert.Equal(2, images.Count);
            Assert.Single(images, x => x.IsPrimary);

            using var setPrimary = await owner.PostAsync($"/profile/listings/{listingId}/images/{images[1].Id}/primary", new FormUrlEncodedContent([]));
            Assert.Equal(HttpStatusCode.Redirect, setPrimary.StatusCode);
            images = await GetImagesAsync(factory, listingId);
            Assert.Equal(images[1].Id, images.Single(x => x.IsPrimary).Id);

            using var readAsOwner = await owner.GetAsync($"/listing-images/{images[1].Id}");
            Assert.Equal(HttpStatusCode.OK, readAsOwner.StatusCode);
            Assert.Equal("image/png", readAsOwner.Content.Headers.ContentType?.MediaType);
            Assert.Equal(Png, await readAsOwner.Content.ReadAsByteArrayAsync());

            using var anonymous = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
            using var hiddenDraftImage = await anonymous.GetAsync($"/listing-images/{images[0].Id}");
            Assert.Equal(HttpStatusCode.NotFound, hiddenDraftImage.StatusCode);

            using var otherUser = AntiforgeryTestClient.Create(factory);
            using var registration = await otherUser.PostAsync("/account/register", new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["DisplayName"] = "Other User", ["Email"] = "images.other@example.com",
                ["Password"] = "StrongPassword123!", ["ConfirmPassword"] = "StrongPassword123!", ["AcceptedTerms"] = "true", ["ConsentedPersonalDataProcessing"] = "true", ["TermsVersion"] = "1.0.0", ["PrivacyVersion"] = "1.0.0"
            }));
            Assert.Equal(HttpStatusCode.Redirect, registration.StatusCode);
            using var forbiddenUpload = await UploadAsync(otherUser, listingId, "intrusion.jpg", Jpeg);
            Assert.Equal(HttpStatusCode.NotFound, forbiddenUpload.StatusCode);

            using var delete = await owner.PostAsync($"/profile/listings/{listingId}/images/{images[1].Id}/delete", new FormUrlEncodedContent([]));
            Assert.Equal(HttpStatusCode.Redirect, delete.StatusCode);
            images = await GetImagesAsync(factory, listingId);
            Assert.Single(images);
            Assert.True(images[0].IsPrimary);

            using var deleteListing = await owner.PostAsync($"/profile/listings/{listingId}/delete", new FormUrlEncodedContent([]));
            Assert.Equal(HttpStatusCode.Redirect, deleteListing.StatusCode);
            Assert.Empty(Directory.EnumerateFiles(storage));
        }
        finally
        {
            if (Directory.Exists(storage)) Directory.Delete(storage, recursive: true);
        }
    }

    [Theory]
    [InlineData("image.jpg", "text/plain")]
    [InlineData("image.exe", "image/jpeg")]
    public async Task RejectsInvalidExtensionOrMimeWithoutPersisting(string name, string contentType)
    {
        var storage = Path.Combine(Path.GetTempPath(), "haven-image-tests", Guid.NewGuid().ToString("N"));
        using var factory = CreateFactory(storage);
        try
        {
            using var owner = AntiforgeryTestClient.Create(factory);
            var listingId = await CreateListingAsync(factory, owner, $"invalid.{Guid.NewGuid():N}@example.com");
            using var response = await UploadAsync(owner, listingId, name, Jpeg, contentType);
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            Assert.Empty(await GetImagesAsync(factory, listingId));
            Assert.False(Directory.Exists(storage) && Directory.EnumerateFiles(storage).Any());
        }
        finally
        {
            if (Directory.Exists(storage)) Directory.Delete(storage, recursive: true);
        }
    }

    [Fact]
    public async Task RejectsSpoofedImageContentsAndOversizedFiles()
    {
        var storage = Path.Combine(Path.GetTempPath(), "haven-image-tests", Guid.NewGuid().ToString("N"));
        using var factory = CreateFactory(storage);
        try
        {
            using var owner = AntiforgeryTestClient.Create(factory);
            var listingId = await CreateListingAsync(factory, owner, "spoof.owner@example.com");
            using var fake = await UploadAsync(owner, listingId, "not-an-image.jpg", "not an image"u8.ToArray());
            Assert.Equal(HttpStatusCode.Redirect, fake.StatusCode);
            var large = new byte[10 * 1024 * 1024 + 1];
            large[0] = 0xff; large[1] = 0xd8; large[2] = 0xff;
            using var tooLarge = await UploadAsync(owner, listingId, "large.jpg", large);
            Assert.Equal(HttpStatusCode.Redirect, tooLarge.StatusCode);
            Assert.Empty(await GetImagesAsync(factory, listingId));
        }
        finally
        {
            if (Directory.Exists(storage)) Directory.Delete(storage, recursive: true);
        }
    }

    [Fact]
    public async Task DatabaseFailureAfterFileWriteRemovesTheStoredFile()
    {
        var storage = Path.Combine(Path.GetTempPath(), "haven-image-tests", Guid.NewGuid().ToString("N"));
        using var factory = CreateFactory(storage, failImageSave: true);
        try
        {
            using var owner = AntiforgeryTestClient.Create(factory);
            var listingId = await CreateListingAsync(factory, owner, "rollback.owner@example.com");
            using var response = await UploadAsync(owner, listingId, "failure.jpg", Jpeg);
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            Assert.Empty(await GetImagesAsync(factory, listingId));
            Assert.True(Directory.Exists(storage));
            Assert.Empty(Directory.EnumerateFiles(storage));
        }
        finally
        {
            if (Directory.Exists(storage)) Directory.Delete(storage, recursive: true);
        }
    }

    [Fact]
    public async Task ListingCannotExceedTenImages()
    {
        var storage = Path.Combine(Path.GetTempPath(), "haven-image-tests", Guid.NewGuid().ToString("N"));
        using var factory = CreateFactory(storage);
        try
        {
            using var owner = AntiforgeryTestClient.Create(factory);
            var listingId = await CreateListingAsync(factory, owner, "image.limit@example.com");
            using var form = new MultipartFormDataContent();
            for (var index = 0; index < 11; index++)
            {
                var file = new ByteArrayContent(Jpeg);
                file.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
                form.Add(file, "images", $"house-{index}.jpg");
            }
            using var response = await owner.PostAsync($"/profile/listings/{listingId}/images", form);
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            Assert.Empty(await GetImagesAsync(factory, listingId));
            Assert.False(Directory.Exists(storage));
        }
        finally
        {
            if (Directory.Exists(storage)) Directory.Delete(storage, recursive: true);
        }
    }

    [Fact]
    public async Task FileStorageFailureAndPendingListingAreReportedWithoutMetadata()
    {
        var blockedRoot = Path.Combine(Path.GetTempPath(), "haven-image-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Path.GetDirectoryName(blockedRoot)!);
        await File.WriteAllTextAsync(blockedRoot, "not a directory");
        using (var factory = CreateFactory(blockedRoot))
        {
            using var owner = AntiforgeryTestClient.Create(factory);
            var listingId = await CreateListingAsync(factory, owner, "storage.failure@example.com");
            using var response = await UploadAsync(owner, listingId, "write-failure.jpg", Jpeg);
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            Assert.Empty(await GetImagesAsync(factory, listingId));
        }
        File.Delete(blockedRoot);

        var storage = Path.Combine(Path.GetTempPath(), "haven-image-tests", Guid.NewGuid().ToString("N"));
        using var pendingFactory = CreateFactory(storage);
        try
        {
            using var owner = AntiforgeryTestClient.Create(pendingFactory);
            var listingId = await CreateListingAsync(pendingFactory, owner, "pending.owner@example.com");
            using var submit = await owner.PostAsync($"/profile/listings/{listingId}/submit", new FormUrlEncodedContent([]));
            Assert.Equal(HttpStatusCode.Redirect, submit.StatusCode);
            using var response = await UploadAsync(owner, listingId, "pending.jpg", Jpeg);
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            Assert.Empty(await GetImagesAsync(pendingFactory, listingId));
        }
        finally
        {
            if (Directory.Exists(storage)) Directory.Delete(storage, recursive: true);
        }
    }

    [Fact]
    public async Task AdminCanManageApprovedListingImagesAndPublicPagesRenderGalleryAndCardImage()
    {
        var storage = Path.Combine(Path.GetTempPath(), "haven-image-tests", Guid.NewGuid().ToString("N"));
        using var factory = CreateFactory(storage, provisionAdmin: true);
        try
        {
            using var owner = AntiforgeryTestClient.Create(factory);
            var listingId = await CreateListingAsync(factory, owner, "public.image.owner@example.com");
            using var upload = await UploadAsync(owner, listingId, "front.jpg", Jpeg);
            Assert.Equal(HttpStatusCode.Redirect, upload.StatusCode);
            using var submit = await owner.PostAsync($"/profile/listings/{listingId}/submit", new FormUrlEncodedContent([]));
            Assert.Equal(HttpStatusCode.Redirect, submit.StatusCode);

            using var admin = AntiforgeryTestClient.Create(factory);
            using var login = await admin.PostAsync("/account/login", new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["Email"] = "media.admin@example.com", ["Password"] = "StrongPassword123!"
            }));
            Assert.Equal(HttpStatusCode.Redirect, login.StatusCode);
            using var approve = await admin.PostAsync($"/Admin/Listings/Approve/{listingId}", new FormUrlEncodedContent([]));
            Assert.Equal(HttpStatusCode.Redirect, approve.StatusCode);

            using var detail = await owner.GetAsync($"/properties/{listingId}");
            Assert.Equal(HttpStatusCode.OK, detail.StatusCode);
            Assert.Contains($"/listing-images/", await detail.Content.ReadAsStringAsync());
            using var search = await owner.GetAsync("/properties");
            Assert.Contains("property-card__image", await search.Content.ReadAsStringAsync());

            var images = await GetImagesAsync(factory, listingId);
            using var adminUpload = await UploadAsync(admin, listingId, "admin-added.png", Png, "image/png", "/Admin/Listings/UploadImages");
            Assert.Equal(HttpStatusCode.Redirect, adminUpload.StatusCode);
            images = await GetImagesAsync(factory, listingId);
            Assert.Equal(2, images.Count);
            using var adminDelete = await admin.PostAsync($"/Admin/Listings/DeleteImage/{listingId}?imageId={images[1].Id}", new FormUrlEncodedContent([]));
            Assert.Equal(HttpStatusCode.Redirect, adminDelete.StatusCode);
            Assert.Single(await GetImagesAsync(factory, listingId));
        }
        finally
        {
            if (Directory.Exists(storage)) Directory.Delete(storage, recursive: true);
        }
    }

    private static readonly byte[] Jpeg = [0xff, 0xd8, 0xff, 0x01, 0x02, 0xff, 0xd9];
    private static readonly byte[] Png = [137, 80, 78, 71, 13, 10, 26, 10, 0, 0, 0, 0, 73, 69, 78, 68, 0, 0, 0, 0];

    private static async Task<int> CreateListingAsync(WebApplicationFactory<Program> factory, HttpClient client, string email)
    {
        using var registration = await client.PostAsync("/account/register", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["DisplayName"] = "Listing Owner", ["Email"] = email,
            ["Password"] = "StrongPassword123!", ["ConfirmPassword"] = "StrongPassword123!", ["AcceptedTerms"] = "true", ["ConsentedPersonalDataProcessing"] = "true", ["TermsVersion"] = "1.0.0", ["PrivacyVersion"] = "1.0.0"
        }));
        Assert.Equal(HttpStatusCode.Redirect, registration.StatusCode);
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            db.PropertyTypes.Add(PropertyType.Create($"Home-{Guid.NewGuid():N}"));
            await db.SaveChangesAsync();
        }
        var form = new MultipartFormDataContent();
        foreach (var pair in new Dictionary<string, string>
        {
            ["PropertyTypeId"] = "1", ["Title"] = "Image test listing", ["Description"] = "Test",
            ["Purpose"] = "1", ["PriceAmount"] = "1000000", ["AreaSqm"] = "50",
            ["Province"] = "Hà Nội", ["District"] = "Phường Ba Đình", ["ProvinceCode"] = "01", ["WardCode"] = "00004", ["AddressLine"] = "1 Test Street"
        }) form.Add(new StringContent(pair.Value), pair.Key);
        using var response = await client.PostAsync("/profile/listings/create", form);
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        var location = response.Headers.Location?.OriginalString ?? throw new InvalidOperationException("Listing redirect missing.");
        return int.Parse(location.Split('/').Last());
    }

    private static async Task<HttpResponseMessage> UploadAsync(HttpClient client, int listingId, string name, byte[] bytes, string contentType = "image/jpeg", string? route = null)
    {
        var form = new MultipartFormDataContent();
        var file = new ByteArrayContent(bytes);
        file.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        form.Add(file, "images", name);
        return await client.PostAsync(route is null ? $"/profile/listings/{listingId}/images" : $"{route}/{listingId}", form);
    }

    private static async Task<IReadOnlyList<Haven.Application.Listings.ListingImageInfo>> GetImagesAsync(WebApplicationFactory<Program> factory, int listingId)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        return await db.ListingImages.AsNoTracking().Where(x => x.ListingId == listingId)
            .OrderBy(x => x.SortOrder).Select(x => new Haven.Application.Listings.ListingImageInfo(x.Id, x.ListingId, x.ContentType, x.Length, x.SortOrder, x.IsPrimary)).ToArrayAsync();
    }

    private static WebApplicationFactory<Program> CreateFactory(string storagePath, bool failImageSave = false, bool provisionAdmin = false)
    {
        var databaseName = Guid.NewGuid().ToString();
        return new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseSetting("Haven:ListingImages:StoragePath", storagePath);
            if (provisionAdmin)
            {
                builder.UseSetting("Haven:AdminBootstrap:Email", "media.admin@example.com");
                builder.UseSetting("Haven:AdminBootstrap:Password", "StrongPassword123!");
            }
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<IDbContextOptionsConfiguration<ApplicationDbContext>>();
                services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
                services.RemoveAll<ApplicationDbContext>();
                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseInMemoryDatabase(databaseName);
                    if (failImageSave) options.AddInterceptors(new FailImageSaveInterceptor());
                });
            });
        });
    }

    private sealed class FailImageSaveInterceptor : SaveChangesInterceptor
    {
        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            if (eventData.Context?.ChangeTracker.Entries<ListingImage>().Any(entry => entry.State == EntityState.Added) == true)
                throw new IOException("Simulated metadata persistence failure.");
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }
    }
}
