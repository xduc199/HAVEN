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

public sealed class FavoritesApiTests
{
    [Fact]
    public async Task Favorites_AreUserScopedAndRejectDuplicatesOrPrivateListings()
    {
        using var factory = CreateFactory();
        await SeedListingsAsync(factory);
        using var client = CreateClient(factory);
        await RegisterAsync(client, "favorite.user@example.com");

        using var addResponse = await client.PostAsync(
            "/api/favorites/1",
            content: null);
        using var duplicateResponse = await client.PostAsync(
            "/api/favorites/1",
            content: null);
        using var pendingResponse = await client.PostAsync(
            "/api/favorites/2",
            content: null);
        using var invalidIdResponse = await client.PostAsync(
            "/api/favorites/0",
            content: null);
        using var invalidPageResponse = await client.GetAsync(
            "/api/favorites?page=2147483647&pageSize=100");
        using var getResponse = await client.GetAsync(
            "/api/favorites?page=1&pageSize=10");

        Assert.Equal(HttpStatusCode.Created, addResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, duplicateResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, pendingResponse.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, invalidIdResponse.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, invalidPageResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        using var body = JsonDocument.Parse(
            await getResponse.Content.ReadAsStringAsync());
        var root = body.RootElement;
        Assert.Equal(1, root.GetProperty("totalCount").GetInt32());
        Assert.Equal(1, root.GetProperty("items").GetArrayLength());
        Assert.Equal(
            "Approved listing",
            root.GetProperty("items")[0].GetProperty("title").GetString());
    }

    [Fact]
    public async Task Favorite_CanOnlyBeRemovedByItsOwner()
    {
        using var factory = CreateFactory();
        await SeedListingsAsync(factory);
        using var client = CreateClient(factory);
        await RegisterAsync(client, "first.favorite@example.com");

        using var addResponse = await client.PostAsync(
            "/api/favorites/1",
            content: null);
        Assert.Equal(HttpStatusCode.Created, addResponse.StatusCode);

        await RegisterAsync(client, "second.favorite@example.com");

        using var unauthorizedRemoveResponse = await client.DeleteAsync(
            "/api/favorites/1");
        using var emptyFavoritesResponse = await client.GetAsync(
            "/api/favorites");

        Assert.Equal(
            HttpStatusCode.NotFound,
            unauthorizedRemoveResponse.StatusCode);
        using var body = JsonDocument.Parse(
            await emptyFavoritesResponse.Content.ReadAsStringAsync());
        Assert.Equal(0, body.RootElement.GetProperty("totalCount").GetInt32());
    }

    [Fact]
    public async Task Favorites_RequireAuthentication()
    {
        using var factory = CreateFactory();
        using var client = CreateClient(factory);

        using var getResponse = await client.GetAsync("/api/favorites");
        using var addResponse = await client.PostAsync(
            "/api/favorites/1",
            content: null);

        Assert.Equal(HttpStatusCode.Unauthorized, getResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, addResponse.StatusCode);
    }

    private static async Task SeedListingsAsync(
        WebApplicationFactory<Program> factory)
    {
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider
            .GetRequiredService<ApplicationDbContext>();
        dbContext.PropertyTypes.Add(PropertyType.Create("House"));

        var approved = CreateListing("Approved listing");
        approved.SubmitForReview(DateTimeOffset.UtcNow);
        approved.Approve(DateTimeOffset.UtcNow);
        dbContext.Listings.Add(approved);
        dbContext.Listings.Add(CreateListing("Pending listing"));
        await dbContext.SaveChangesAsync();
    }

    private static Listing CreateListing(string title) =>
        Listing.Create(
            "favorite-owner",
            1,
            title,
            "Description",
            ListingPurpose.Sale,
            1_000_000m,
            50m,
            "Province",
            "District",
            "Address",
            DateTimeOffset.UtcNow);

    private static async Task RegisterAsync(
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
