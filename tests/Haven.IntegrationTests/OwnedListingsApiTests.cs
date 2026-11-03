using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Haven.Domain.Properties;
using Haven.Infrastructure.Context;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Haven.IntegrationTests;

public sealed class OwnedListingsApiTests
{
    [Fact]
    public async Task Create_WithoutAuthentication_ReturnsUnauthorized()
    {
        using var factory = CreateFactory();
        using var client = CreateClient(factory);

        using var response = await client.PostAsJsonAsync(
            "/api/my/listings",
            CreateListingRequest());

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Owner_CanCreateReadUpdateAndSubmitListing()
    {
        using var factory = CreateFactory();
        await SeedPropertyTypeAsync(factory);
        using var client = CreateClient(factory);
        var accountId = await RegisterAsync(client, "owner@example.com");

        using var createResponse = await client.PostAsJsonAsync(
            "/api/my/listings",
            new
            {
                propertyTypeId = 1,
                title = "House for sale",
                description = "Near the city center",
                purpose = ListingPurpose.Sale,
                priceAmount = 1_000_000m,
                areaSqm = 50m,
                province = "Province",
                district = "District",
                addressLine = "1 Main Street",
                ownerId = "client-controlled-owner"
            });

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        Assert.NotNull(createResponse.Headers.Location);
        using var createdBody = JsonDocument.Parse(
            await createResponse.Content.ReadAsStringAsync());
        var created = createdBody.RootElement;
        var listingId = created.GetProperty("id").GetInt32();
        Assert.Equal("House for sale", created.GetProperty("title").GetString());
        Assert.False(created.TryGetProperty("ownerId", out _));
        Assert.Equal(
            accountId,
            await GetStoredOwnerIdAsync(factory, listingId));

        using var getResponse = await client.GetAsync(
            $"/api/my/listings/{listingId}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        using var updateResponse = await client.PutAsJsonAsync(
            $"/api/my/listings/{listingId}",
            CreateListingRequest(title: "Updated house"));
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        using var updatedBody = JsonDocument.Parse(
            await updateResponse.Content.ReadAsStringAsync());
        Assert.Equal(
            "Updated house",
            updatedBody.RootElement.GetProperty("title").GetString());

        using var submitResponse = await client.PostAsync(
            $"/api/my/listings/{listingId}/submit",
            content: null);
        Assert.Equal(HttpStatusCode.OK, submitResponse.StatusCode);
        using var submittedBody = JsonDocument.Parse(
            await submitResponse.Content.ReadAsStringAsync());
        Assert.Equal(
            (int)ListingStatus.Pending,
            submittedBody.RootElement.GetProperty("status").GetInt32());

        using var secondSubmitResponse = await client.PostAsync(
            $"/api/my/listings/{listingId}/submit",
            content: null);
        using var publicResponse = await client.GetAsync(
            $"/api/listings/{listingId}");
        using var deletePendingResponse = await client.DeleteAsync(
            $"/api/my/listings/{listingId}");
        Assert.Equal(HttpStatusCode.Conflict, secondSubmitResponse.StatusCode);
        Assert.Equal(
            HttpStatusCode.Conflict,
            deletePendingResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, publicResponse.StatusCode);
    }

    [Fact]
    public async Task OwnerListingCollection_IsPagedAndScopedToAuthenticatedOwner()
    {
        using var factory = CreateFactory();
        await SeedPropertyTypeAsync(factory);
        using var ownerClient = CreateClient(factory);
        await RegisterAsync(ownerClient, "collection.owner@example.com");

        using var createResponse = await ownerClient.PostAsJsonAsync(
            "/api/my/listings",
            CreateListingRequest(title: "Private owner's draft"));
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        using var ownerResponse = await ownerClient.GetAsync(
            "/api/my/listings?page=1&pageSize=10");
        Assert.Equal(HttpStatusCode.OK, ownerResponse.StatusCode);
        using var ownerBody = JsonDocument.Parse(
            await ownerResponse.Content.ReadAsStringAsync());
        var ownerItems = ownerBody.RootElement.GetProperty("items");
        Assert.Equal(1, ownerItems.GetArrayLength());
        Assert.Equal(
            "Private owner's draft",
            ownerItems[0].GetProperty("title").GetString());
        Assert.Equal(1, ownerBody.RootElement.GetProperty("totalCount").GetInt32());

        using var otherClient = CreateClient(factory);
        await RegisterAsync(otherClient, "collection.other@example.com");
        using var otherResponse = await otherClient.GetAsync("/api/my/listings");
        Assert.Equal(HttpStatusCode.OK, otherResponse.StatusCode);
        using var otherBody = JsonDocument.Parse(
            await otherResponse.Content.ReadAsStringAsync());
        Assert.Empty(otherBody.RootElement.GetProperty("items").EnumerateArray());
    }

    [Fact]
    public async Task Owner_CanDeleteDraftAndRejectedListings()
    {
        using var factory = CreateFactory();
        await SeedPropertyTypeAsync(factory);
        using var client = CreateClient(factory);
        var ownerId = await RegisterAsync(client, "delete.owner@example.com");

        using var createResponse = await client.PostAsJsonAsync(
            "/api/my/listings",
            CreateListingRequest());
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        using var createdBody = JsonDocument.Parse(
            await createResponse.Content.ReadAsStringAsync());
        var draftId = createdBody.RootElement.GetProperty("id").GetInt32();

        using var deleteDraftResponse = await client.DeleteAsync(
            $"/api/my/listings/{draftId}");
        Assert.Equal(HttpStatusCode.NoContent, deleteDraftResponse.StatusCode);

        var rejectedId = await SeedRejectedListingAsync(factory, ownerId);
        using var deleteRejectedResponse = await client.DeleteAsync(
            $"/api/my/listings/{rejectedId}");
        Assert.Equal(
            HttpStatusCode.NoContent,
            deleteRejectedResponse.StatusCode);
        Assert.False(await ListingExistsAsync(factory, rejectedId));
    }

    [Fact]
    public async Task DifferentAccount_CannotReadOwnedListing()
    {
        using var factory = CreateFactory();
        await SeedPropertyTypeAsync(factory);
        using var client = CreateClient(factory);
        await RegisterAsync(client, "first.owner@example.com");

        using var createResponse = await client.PostAsJsonAsync(
            "/api/my/listings",
            CreateListingRequest());
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        using var createdBody = JsonDocument.Parse(
            await createResponse.Content.ReadAsStringAsync());
        var listingId = createdBody.RootElement.GetProperty("id").GetInt32();

        await RegisterAsync(client, "second.owner@example.com");

        using var response = await client.GetAsync(
            $"/api/my/listings/{listingId}");
        using var updateResponse = await client.PutAsJsonAsync(
            $"/api/my/listings/{listingId}",
            CreateListingRequest(title: "Hijacked title"));
        using var submitResponse = await client.PostAsync(
            $"/api/my/listings/{listingId}/submit",
            content: null);
        using var deleteResponse = await client.DeleteAsync(
            $"/api/my/listings/{listingId}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, updateResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, submitResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, deleteResponse.StatusCode);
    }

    [Fact]
    public async Task Create_WithInvalidPrice_ReturnsValidationProblem()
    {
        using var factory = CreateFactory();
        await SeedPropertyTypeAsync(factory);
        using var client = CreateClient(factory);
        await RegisterAsync(client, "invalid.listing@example.com");

        using var response = await client.PostAsJsonAsync(
            "/api/my/listings",
            CreateListingRequest(priceAmount: 0m));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private static object CreateListingRequest(
        string title = "House for sale",
        decimal priceAmount = 1_000_000m) =>
        new
        {
            propertyTypeId = 1,
            title,
            description = "Near the city center",
            purpose = ListingPurpose.Sale,
            priceAmount,
            areaSqm = 50m,
            province = "Province",
            district = "District",
            addressLine = "1 Main Street"
        };

    private static async Task<string> RegisterAsync(
        HttpClient client,
        string email)
    {
        using var response = await client.PostAsJsonAsync(
            "/api/auth/register",
            new
            {
                displayName = email.Split('@')[0],
                email,
                password = "StrongPassword123!"
            });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        using var body = JsonDocument.Parse(
            await response.Content.ReadAsStringAsync());
        return body.RootElement.GetProperty("id").GetString()!;
    }

    private static async Task SeedPropertyTypeAsync(
        WebApplicationFactory<Program> factory)
    {
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider
            .GetRequiredService<ApplicationDbContext>();
        dbContext.PropertyTypes.Add(PropertyType.Create("House"));
        await dbContext.SaveChangesAsync();
    }

    private static async Task<string?> GetStoredOwnerIdAsync(
        WebApplicationFactory<Program> factory,
        int listingId)
    {
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider
            .GetRequiredService<ApplicationDbContext>();
        return await dbContext.Listings
            .Where(listing => listing.Id == listingId)
            .Select(listing => listing.OwnerId)
            .SingleOrDefaultAsync();
    }

    private static async Task<int> SeedRejectedListingAsync(
        WebApplicationFactory<Program> factory,
        string ownerId)
    {
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider
            .GetRequiredService<ApplicationDbContext>();
        var listing = Listing.Create(
            ownerId,
            1,
            "Rejected listing",
            "Description",
            ListingPurpose.Sale,
            1_000_000m,
            50m,
            "Province",
            "District",
            "Address",
            DateTimeOffset.UtcNow);
        listing.SubmitForReview(DateTimeOffset.UtcNow);
        listing.Reject("Needs correction", DateTimeOffset.UtcNow);
        dbContext.Listings.Add(listing);
        await dbContext.SaveChangesAsync();
        return listing.Id;
    }

    private static async Task<bool> ListingExistsAsync(
        WebApplicationFactory<Program> factory,
        int listingId)
    {
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider
            .GetRequiredService<ApplicationDbContext>();
        return await dbContext.Listings
            .AnyAsync(listing => listing.Id == listingId);
    }

    private static WebApplicationFactory<Program> CreateFactory()
    {
        var databaseName = Guid.NewGuid().ToString();
        return new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<
                    IDbContextOptionsConfiguration<ApplicationDbContext>>();
                services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
                services.RemoveAll<ApplicationDbContext>();
                services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseInMemoryDatabase(databaseName));
            }));
    }

    private static HttpClient CreateClient(
        WebApplicationFactory<Program> factory) =>
        AntiforgeryTestClient.Create(factory, new WebApplicationFactoryClientOptions
        {
            HandleCookies = true,
            AllowAutoRedirect = false
        });
}
