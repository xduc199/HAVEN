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

public sealed class PublicListingSearchApiTests
{
    [Fact]
    public async Task Search_CombinesFiltersAndReturnsApprovedListingsOnly()
    {
        using var factory = CreateFactory();
        await SeedListingsAsync(factory,
        [
            CreateListing(
                "Loft by the lake",
                ListingPurpose.Sale,
                100m,
                50m,
                "North",
                "Central",
                2,
                approved: true),
            CreateListing(
                "Loft for rent",
                ListingPurpose.Rent,
                80m,
                60m,
                "North",
                "Central",
                2,
                approved: true),
            CreateListing(
                "Loft awaiting review",
                ListingPurpose.Sale,
                95m,
                50m,
                "North",
                "Central",
                2,
                approved: false)
        ]);
        using var client = factory.CreateClient();

        using var response = await client.GetAsync(
            "/api/listings?keyword=Loft&purpose=Sale&province=North" +
            "&district=Central&minimumPrice=90&maximumPrice=110&bedrooms=2");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var body = JsonDocument.Parse(
            await response.Content.ReadAsStringAsync());
        var root = body.RootElement;
        Assert.Equal(1, root.GetProperty("totalCount").GetInt32());
        var items = root.GetProperty("items");
        Assert.Equal(1, items.GetArrayLength());
        Assert.Equal(
            "Loft by the lake",
            items[0].GetProperty("title").GetString());
    }

    [Fact]
    public async Task Search_SortsAndPaginatesWithStableTotalMetadata()
    {
        using var factory = CreateFactory();
        await SeedListingsAsync(factory,
        [
            CreateListing("Highest price", ListingPurpose.Sale, 300m, 60m, "North", "Central", 2, true),
            CreateListing("Lowest price", ListingPurpose.Sale, 100m, 60m, "North", "Central", 2, true),
            CreateListing("Middle price", ListingPurpose.Sale, 200m, 60m, "North", "Central", 2, true)
        ]);
        using var client = factory.CreateClient();

        using var response = await client.GetAsync(
            "/api/listings?sort=PriceAscending&page=1&pageSize=2");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var body = JsonDocument.Parse(
            await response.Content.ReadAsStringAsync());
        var root = body.RootElement;
        var items = root.GetProperty("items");
        Assert.Equal(2, items.GetArrayLength());
        Assert.Equal(
            "Lowest price",
            items[0].GetProperty("title").GetString());
        Assert.Equal(
            "Middle price",
            items[1].GetProperty("title").GetString());
        Assert.Equal(3, root.GetProperty("totalCount").GetInt32());
        Assert.Equal(2, root.GetProperty("totalPages").GetInt32());
    }

    [Fact]
    public async Task Search_WithInvertedPriceRange_ReturnsBadRequest()
    {
        using var factory = CreateFactory();
        using var client = factory.CreateClient();

        using var response = await client.GetAsync(
            "/api/listings?minimumPrice=200&maximumPrice=100");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PublicTagEndpoints_ExposeOnlyActiveTagsWithApprovedListings()
    {
        using var factory = CreateFactory();
        var approvedTagged = CreateListing(
            "Approved tagged",
            ListingPurpose.Sale,
            100m,
            50m,
            "North",
            "Central",
            2,
            approved: true);
        var approvedUntagged = CreateListing(
            "Approved untagged",
            ListingPurpose.Sale,
            110m,
            55m,
            "North",
            "Central",
            2,
            approved: true);
        var pendingTagged = CreateListing(
            "Pending tagged",
            ListingPurpose.Sale,
            120m,
            60m,
            "North",
            "Central",
            2,
            approved: false);

        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider
                .GetRequiredService<ApplicationDbContext>();
            dbContext.PropertyTypes.Add(PropertyType.Create("House"));
            dbContext.Listings.AddRange(approvedTagged, approvedUntagged, pendingTagged);
            var publicTag = Tag.Create("Public tag", "PUBLIC TAG", "public-tag", null, DateTimeOffset.UtcNow);
            var privateTag = Tag.Create("Pending only", "PENDING ONLY", "pending-only", null, DateTimeOffset.UtcNow);
            dbContext.Tags.AddRange(publicTag, privateTag);
            await dbContext.SaveChangesAsync();
            dbContext.ListingTags.AddRange(
                ListingTag.Create(approvedTagged.Id, publicTag.Id),
                ListingTag.Create(pendingTagged.Id, publicTag.Id),
                ListingTag.Create(pendingTagged.Id, privateTag.Id));
            await dbContext.SaveChangesAsync();
        }

        using var client = factory.CreateClient();
        using var tagResponse = await client.GetAsync("/api/tags");
        using var tagsBody = JsonDocument.Parse(await tagResponse.Content.ReadAsStringAsync());
        var publicTags = tagsBody.RootElement.GetProperty("items");
        Assert.Equal(HttpStatusCode.OK, tagResponse.StatusCode);
        Assert.Single(publicTags.EnumerateArray());
        Assert.Equal("public-tag", publicTags[0].GetProperty("slug").GetString());

        using var filtered = await client.GetAsync("/api/tags/public-tag/listings");
        using var filteredBody = JsonDocument.Parse(await filtered.Content.ReadAsStringAsync());
        Assert.Equal(HttpStatusCode.OK, filtered.StatusCode);
        Assert.Equal(1, filteredBody.RootElement.GetProperty("totalCount").GetInt32());
        Assert.Equal(
            "Approved tagged",
            filteredBody.RootElement.GetProperty("items")[0].GetProperty("title").GetString());

        using var hiddenTag = await client.GetAsync("/api/tags/pending-only/listings");
        Assert.Equal(HttpStatusCode.NotFound, hiddenTag.StatusCode);
    }

    private static Listing CreateListing(
        string title,
        ListingPurpose purpose,
        decimal priceAmount,
        decimal areaSqm,
        string province,
        string district,
        int bedrooms,
        bool approved)
    {
        var listing = Listing.Create(
            "search-owner",
            1,
            title,
            "Listing description",
            purpose,
            priceAmount,
            areaSqm,
            province,
            district,
            "1 Main Street",
            new DateTimeOffset(2026, 11, 3, 12, 0, 0, TimeSpan.Zero),
            bedrooms);

        if (approved)
        {
            listing.SubmitForReview(DateTimeOffset.UtcNow);
            listing.Approve(DateTimeOffset.UtcNow);
        }

        return listing;
    }

    private static async Task SeedListingsAsync(
        WebApplicationFactory<Program> factory,
        IReadOnlyCollection<Listing> listings)
    {
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider
            .GetRequiredService<ApplicationDbContext>();
        dbContext.PropertyTypes.Add(PropertyType.Create("House"));
        dbContext.Listings.AddRange(listings);
        await dbContext.SaveChangesAsync();
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
}
