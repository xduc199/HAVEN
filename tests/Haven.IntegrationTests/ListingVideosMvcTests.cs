using System.Net;
using System.Net.Http.Headers;
using Haven.Domain.Properties;
using Haven.Infrastructure.Context;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Haven.IntegrationTests;

public sealed class ListingVideosMvcTests
{
    private static readonly byte[] Mp4 = [0, 0, 0, 24, (byte)'f', (byte)'t', (byte)'y', (byte)'p', (byte)'i', (byte)'s', (byte)'o', (byte)'m', 0, 0, 0, 0, (byte)'i', (byte)'s', (byte)'o', (byte)'m', 0, 0, 0, 0];
    private static readonly byte[] Webm = [0x1a, 0x45, 0xdf, 0xa3, 0x42, 0x86, 0x81, 0x01, 0x42, 0xf7, 0x81, 0x01];

    [Fact]
    public async Task OwnerUploadsMp4AndWebmAndPublicStreamSupportsRanges()
    {
        var storage = Path.Combine(Path.GetTempPath(), "haven-video-tests", Guid.NewGuid().ToString("N"));
        using var factory = CreateFactory(storage, provisionAdmin: true);
        try
        {
            using var owner = AntiforgeryTestClient.Create(factory);
            var listingId = await CreateListingAsync(factory, owner, "video.owner@example.com");
            using var mp4Upload = await UploadAsync(owner, listingId, "tour.mp4", Mp4, "video/mp4");
            using var webmUpload = await UploadAsync(owner, listingId, "walkthrough.webm", Webm, "video/webm");
            Assert.Equal(HttpStatusCode.Redirect, mp4Upload.StatusCode);
            Assert.Equal(HttpStatusCode.Redirect, webmUpload.StatusCode);

            var videos = await GetVideosAsync(factory, listingId);
            Assert.Equal(2, videos.Count);
            Assert.Contains(videos, x => x.ContentType == "video/mp4");
            Assert.Contains(videos, x => x.ContentType == "video/webm");
            using var draftDetail = await owner.GetAsync($"/profile/listings/{listingId}");
            Assert.Contains("listing-videos/", await draftDetail.Content.ReadAsStringAsync());
            using var invalidEdit = await owner.PostAsync($"/profile/listings/{listingId}/edit", new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["Title"] = "", ["Description"] = "Test", ["PropertyTypeId"] = "1", ["Purpose"] = "1",
                ["PriceAmount"] = "1000000", ["AreaSqm"] = "50", ["ProvinceCode"] = "01", ["WardCode"] = "00004", ["AddressLine"] = "1 Test Street"
            }));
            Assert.Equal(HttpStatusCode.OK, invalidEdit.StatusCode);
            Assert.Contains("Video 0", await invalidEdit.Content.ReadAsStringAsync());
            using var hidden = await factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false }).GetAsync($"/listing-videos/{videos[0].Id}");
            Assert.Equal(HttpStatusCode.NotFound, hidden.StatusCode);

            using var other = AntiforgeryTestClient.Create(factory);
            using var registration = await other.PostAsync("/account/register", new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["DisplayName"] = "Other", ["Email"] = "video.other@example.com", ["Password"] = "StrongPassword123!", ["ConfirmPassword"] = "StrongPassword123!", ["AcceptedTerms"] = "true", ["ConsentedPersonalDataProcessing"] = "true", ["TermsVersion"] = "1.1.0", ["PrivacyVersion"] = "1.1.0"
            }));
            Assert.Equal(HttpStatusCode.Redirect, registration.StatusCode);
            using var forbiddenDelete = await other.PostAsync($"/profile/listings/{listingId}/videos/{videos[0].Id}/delete", new FormUrlEncodedContent([]));
            Assert.Equal(HttpStatusCode.NotFound, forbiddenDelete.StatusCode);

            using var image = await UploadImageAsync(owner, listingId);
            Assert.Equal(HttpStatusCode.Redirect, image.StatusCode);
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

            using var publicPage = await factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false }).GetAsync($"/properties/{listingId}");
            Assert.Equal(HttpStatusCode.OK, publicPage.StatusCode);
            Assert.Contains("controls", await publicPage.Content.ReadAsStringAsync());
            using var publicVideo = await factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false }).GetAsync($"/listing-videos/{videos[0].Id}");
            Assert.Equal(HttpStatusCode.OK, publicVideo.StatusCode);
            Assert.Equal("video/mp4", publicVideo.Content.Headers.ContentType?.MediaType);
            using var request = new HttpRequestMessage(HttpMethod.Get, $"/listing-videos/{videos[0].Id}");
            request.Headers.Range = new RangeHeaderValue(0, 3);
            using var range = await factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false }).SendAsync(request);
            Assert.Equal(HttpStatusCode.PartialContent, range.StatusCode);
            Assert.Equal(Mp4[..4], await range.Content.ReadAsByteArrayAsync());

            using var deleteVideo = await owner.PostAsync($"/profile/listings/{listingId}/videos/{videos[0].Id}/delete", new FormUrlEncodedContent([]));
            Assert.Equal(HttpStatusCode.Redirect, deleteVideo.StatusCode);
            Assert.Single(await GetVideosAsync(factory, listingId));
            using (var scope = factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                Assert.Single(await db.ListingImages.Where(x => x.ListingId == listingId).ToListAsync());
            }
            using var noLongerPublic = await factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false }).GetAsync($"/listing-videos/{videos[1].Id}");
            Assert.Equal(HttpStatusCode.NotFound, noLongerPublic.StatusCode);
        }
        finally { if (Directory.Exists(storage)) Directory.Delete(storage, recursive: true); }
    }

    [Theory]
    [InlineData("tour.mp4", "text/plain", true)]
    [InlineData("tour.exe", "video/mp4", true)]
    [InlineData("fake.webm", "video/webm", false)]
    public async Task RejectsMismatchedOrInvalidVideoContainer(string name, string contentType, bool mp4Header)
    {
        var storage = Path.Combine(Path.GetTempPath(), "haven-video-tests", Guid.NewGuid().ToString("N"));
        using var factory = CreateFactory(storage);
        try
        {
            using var owner = AntiforgeryTestClient.Create(factory);
            var listingId = await CreateListingAsync(factory, owner, $"video.invalid.{Guid.NewGuid():N}@example.com");
            var bytes = mp4Header ? Mp4 : "not a video"u8.ToArray();
            using var response = await UploadAsync(owner, listingId, name, bytes, contentType);
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            Assert.Empty(await GetVideosAsync(factory, listingId));
            Assert.False(Directory.Exists(storage) && Directory.EnumerateFiles(storage).Any());
        }
        finally { if (Directory.Exists(storage)) Directory.Delete(storage, recursive: true); }
    }

    [Fact]
    public async Task ConfiguredFileLimitAndMetadataFailureDoNotLeaveMediaFiles()
    {
        var limitedStorage = Path.Combine(Path.GetTempPath(), "haven-video-tests", Guid.NewGuid().ToString("N"));
        using (var factory = CreateFactory(limitedStorage, maximumVideoLength: 16))
        {
            using var owner = AntiforgeryTestClient.Create(factory);
            var listingId = await CreateListingAsync(factory, owner, "video.large@example.com");
            using var response = await UploadAsync(owner, listingId, "large.mp4", Mp4, "video/mp4");
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            Assert.Empty(await GetVideosAsync(factory, listingId));
        }

        var failedStorage = Path.Combine(Path.GetTempPath(), "haven-video-tests", Guid.NewGuid().ToString("N"));
        using (var factory = CreateFactory(failedStorage, failVideoSave: true))
        {
            using var owner = AntiforgeryTestClient.Create(factory);
            var listingId = await CreateListingAsync(factory, owner, "video.dbfail@example.com");
            using var response = await UploadAsync(owner, listingId, "failure.mp4", Mp4, "video/mp4");
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            Assert.Empty(await GetVideosAsync(factory, listingId));
            Assert.True(Directory.Exists(failedStorage));
            Assert.Empty(Directory.EnumerateFiles(failedStorage));
        }
        if (Directory.Exists(limitedStorage)) Directory.Delete(limitedStorage, recursive: true);
        if (Directory.Exists(failedStorage)) Directory.Delete(failedStorage, recursive: true);
    }

    [Fact]
    public async Task AdminCanUploadAndDeleteVideoForManagedListing()
    {
        var storage = Path.Combine(Path.GetTempPath(), "haven-video-tests", Guid.NewGuid().ToString("N"));
        using var factory = CreateFactory(storage, provisionAdmin: true);
        try
        {
            using var owner = AntiforgeryTestClient.Create(factory);
            var listingId = await CreateListingAsync(factory, owner, "video.admin-target@example.com");
            using var admin = AntiforgeryTestClient.Create(factory);
            using var login = await admin.PostAsync("/account/login", new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["Email"] = "media.admin@example.com", ["Password"] = "StrongPassword123!"
            }));
            Assert.Equal(HttpStatusCode.Redirect, login.StatusCode);

            using var upload = await admin.PostAsync($"/Admin/Listings/UploadVideos/{listingId}", new MultipartFormDataContent
            {
                { WithType(Mp4, "video/mp4"), "videos", "admin-tour.mp4" }
            });
            Assert.Equal(HttpStatusCode.Redirect, upload.StatusCode);
            var video = Assert.Single(await GetVideosAsync(factory, listingId));

            using var delete = await admin.PostAsync($"/Admin/Listings/DeleteVideo/{listingId}?videoId={video.Id}", new FormUrlEncodedContent([]));
            Assert.Equal(HttpStatusCode.Redirect, delete.StatusCode);
            Assert.Empty(await GetVideosAsync(factory, listingId));
            Assert.Empty(Directory.EnumerateFiles(storage));
        }
        finally { if (Directory.Exists(storage)) Directory.Delete(storage, recursive: true); }
    }

    [Fact]
    public async Task StorageWriteFailureDoesNotCreateVideoMetadata()
    {
        var parent = Path.Combine(Path.GetTempPath(), "haven-video-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(parent);
        var storageFile = Path.Combine(parent, "not-a-directory");
        await File.WriteAllTextAsync(storageFile, "block storage directory creation");
        using var factory = CreateFactory(storageFile);
        try
        {
            using var owner = AntiforgeryTestClient.Create(factory);
            var listingId = await CreateListingAsync(factory, owner, "video.storage-fail@example.com");
            using var response = await UploadAsync(owner, listingId, "failure.mp4", Mp4, "video/mp4");
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            Assert.Empty(await GetVideosAsync(factory, listingId));
            Assert.Equal("block storage directory creation", await File.ReadAllTextAsync(storageFile));
        }
        finally { if (Directory.Exists(parent)) Directory.Delete(parent, recursive: true); }
    }

    [Fact]
    public async Task EmptyVideoIsRejected()
    {
        var storage = Path.Combine(Path.GetTempPath(), "haven-video-tests", Guid.NewGuid().ToString("N"));
        using var factory = CreateFactory(storage);
        try
        {
            using var owner = AntiforgeryTestClient.Create(factory);
            var listingId = await CreateListingAsync(factory, owner, "video.empty@example.com");
            using var response = await UploadAsync(owner, listingId, "empty.mp4", [], "video/mp4");
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            Assert.Empty(await GetVideosAsync(factory, listingId));
            Assert.False(Directory.Exists(storage) && Directory.EnumerateFiles(storage).Any());
        }
        finally { if (Directory.Exists(storage)) Directory.Delete(storage, recursive: true); }
    }

    [Fact]
    public async Task MissingStoredFileReturnsNotFoundWithoutExposingPhysicalPath()
    {
        var storage = Path.Combine(Path.GetTempPath(), "haven-video-tests", Guid.NewGuid().ToString("N"));
        using var factory = CreateFactory(storage, provisionAdmin: true);
        try
        {
            using var owner = AntiforgeryTestClient.Create(factory);
            var listingId = await CreateListingAsync(factory, owner, "video.missing-file@example.com");
            using var upload = await UploadAsync(owner, listingId, "lost.mp4", Mp4, "video/mp4");
            Assert.Equal(HttpStatusCode.Redirect, upload.StatusCode);
            var video = Assert.Single(await GetVideosAsync(factory, listingId));
            string storageKey;
            using (var scope = factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                storageKey = await db.ListingVideos.Where(x => x.Id == video.Id).Select(x => x.StorageKey).SingleAsync();
                var listing = await db.Listings.SingleAsync(x => x.Id == listingId);
                listing.SubmitForReview(DateTimeOffset.UtcNow);
                listing.Approve(DateTimeOffset.UtcNow.AddSeconds(1));
                await db.SaveChangesAsync();
            }
            File.Delete(Path.Combine(storage, storageKey));
            using var response = await factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false }).GetAsync($"/listing-videos/{video.Id}");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.DoesNotContain(storage, await response.Content.ReadAsStringAsync(), StringComparison.OrdinalIgnoreCase);
        }
        finally { if (Directory.Exists(storage)) Directory.Delete(storage, recursive: true); }
    }

    private static async Task<int> CreateListingAsync(WebApplicationFactory<Program> factory, HttpClient client, string email)
    {
        using var registration = await client.PostAsync("/account/register", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["DisplayName"] = "Video Owner", ["Email"] = email, ["Password"] = "StrongPassword123!", ["ConfirmPassword"] = "StrongPassword123!", ["AcceptedTerms"] = "true", ["ConsentedPersonalDataProcessing"] = "true", ["TermsVersion"] = "1.1.0", ["PrivacyVersion"] = "1.1.0"
        }));
        Assert.Equal(HttpStatusCode.Redirect, registration.StatusCode);
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            db.PropertyTypes.Add(PropertyType.Create($"Video-{Guid.NewGuid():N}"));
            await db.SaveChangesAsync();
        }
        using var create = await client.PostAsync("/profile/listings/create", new MultipartFormDataContent
        {
            { new StringContent("1"), "PropertyTypeId" }, { new StringContent("Video listing"), "Title" },
            { new StringContent("Test"), "Description" }, { new StringContent("1"), "Purpose" },
            { new StringContent("1000000"), "PriceAmount" }, { new StringContent("50"), "AreaSqm" },
            { new StringContent("Hà Nội"), "Province" }, { new StringContent("Phường Ba Đình"), "District" },
            { new StringContent("01"), "ProvinceCode" }, { new StringContent("00004"), "WardCode" },
            { new StringContent("1 Test Street"), "AddressLine" }
        });
        Assert.Equal(HttpStatusCode.Redirect, create.StatusCode);
        return int.Parse(create.Headers.Location!.OriginalString.Split('/').Last());
    }

    private static Task<HttpResponseMessage> UploadAsync(HttpClient client, int listingId, string name, byte[] bytes, string type) =>
        client.PostAsync($"/profile/listings/{listingId}/videos", new MultipartFormDataContent
        {
            { WithType(bytes, type), "videos", name }
        });

    private static Task<HttpResponseMessage> UploadImageAsync(HttpClient client, int listingId) =>
        client.PostAsync($"/profile/listings/{listingId}/images", new MultipartFormDataContent
        {
            { WithType([0xff, 0xd8, 0xff, 0x01, 0x02, 0xff, 0xd9], "image/jpeg"), "images", "front.jpg" }
        });

    private static ByteArrayContent WithType(byte[] bytes, string mimeType)
    {
        var content = new ByteArrayContent(bytes);
        content.Headers.ContentType = new MediaTypeHeaderValue(mimeType);
        return content;
    }

    private static async Task<IReadOnlyList<Haven.Application.Listings.ListingVideoInfo>> GetVideosAsync(WebApplicationFactory<Program> factory, int listingId)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        return await db.ListingVideos.AsNoTracking().Where(x => x.ListingId == listingId).OrderBy(x => x.SortOrder)
            .Select(x => new Haven.Application.Listings.ListingVideoInfo(x.Id, x.ListingId, x.ContentType, x.Length, x.SortOrder)).ToArrayAsync();
    }

    private static WebApplicationFactory<Program> CreateFactory(string path, bool provisionAdmin = false, long maximumVideoLength = 100 * 1024 * 1024, bool failVideoSave = false)
    {
        var dbName = Guid.NewGuid().ToString();
        return new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseSetting("Haven:ListingImages:StoragePath", path);
            builder.UseSetting("Haven:ListingVideos:MaximumFileLengthBytes", maximumVideoLength.ToString());
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
                    options.UseInMemoryDatabase(dbName);
                    if (failVideoSave) options.AddInterceptors(new FailVideoSaveInterceptor());
                });
            });
        });
    }

    private sealed class FailVideoSaveInterceptor : SaveChangesInterceptor
    {
        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            if (eventData.Context?.ChangeTracker.Entries<ListingVideo>().Any(entry => entry.State == EntityState.Added) == true) throw new IOException("Simulated metadata persistence failure.");
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }
    }
}
