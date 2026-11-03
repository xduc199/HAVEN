using System.Net;
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

public sealed class ListingCompareApiTests
{
    [Fact]
    public async Task Compare_ReturnsApprovedListingsInRequestedOrder()
    {
        using var factory = CreateFactory();
        await SeedListingsAsync(factory);
        using var client = factory.CreateClient();

        using var response = await client.GetAsync(
            "/api/listings/compare?ids=2&ids=1");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var body = JsonDocument.Parse(
            await response.Content.ReadAsStringAsync());
        var listings = body.RootElement;
        Assert.Equal(2, listings.GetArrayLength());
        Assert.Equal(
            "Second listing",
            listings[0].GetProperty("title").GetString());
        Assert.Equal(
            "First listing",
            listings[1].GetProperty("title").GetString());
        Assert.False(listings[0].TryGetProperty("ownerId", out _));
    }

    [Fact]
    public async Task Compare_WithMissingOrUnapprovedListing_ReturnsNotFound()
    {
        using var factory = CreateFactory();
        await SeedListingsAsync(factory);
        using var client = factory.CreateClient();

        using var response = await client.GetAsync(
            "/api/listings/compare?ids=1&ids=3");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Theory]
    [InlineData("/api/listings/compare?ids=1&ids=1")]
    [InlineData("/api/listings/compare?ids=1&ids=2&ids=3&ids=4")]
    [InlineData("/api/listings/compare?ids=0")]
    public async Task Compare_WithInvalidIds_ReturnsBadRequest(string route)
    {
        using var factory = CreateFactory();
        using var client = factory.CreateClient();

        using var response = await client.GetAsync(route);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private static async Task SeedListingsAsync(
        WebApplicationFactory<Program> factory)
    {
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider
            .GetRequiredService<ApplicationDbContext>();
        dbContext.PropertyTypes.Add(PropertyType.Create("House"));

        var first = CreateListing("First listing");
        first.SubmitForReview(DateTimeOffset.UtcNow);
        first.Approve(DateTimeOffset.UtcNow);
        var second = CreateListing("Second listing");
        second.SubmitForReview(DateTimeOffset.UtcNow);
        second.Approve(DateTimeOffset.UtcNow);

        dbContext.Listings.AddRange(
            first,
            second,
            CreateListing("Pending listing"));
        await dbContext.SaveChangesAsync();
    }

    private static Listing CreateListing(string title) =>
        Listing.Create(
            "compare-owner",
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
}
