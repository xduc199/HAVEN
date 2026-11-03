using System.Net;
using System.Text.Json;
using Haven.Application.Abstractions;
using Haven.Application.Listings;
using Haven.Application.Tags;
using Haven.Domain.Properties;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Haven.IntegrationTests;

public sealed class PublicListingsApiTests
    : IClassFixture<WebApplicationFactory<Program>>
{
    private static readonly DateTimeOffset Now =
        new(2026, 10, 26, 12, 0, 0, TimeSpan.Zero);

    private readonly WebApplicationFactory<Program> _factory;

    public PublicListingsApiTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetById_WithApprovedListing_ReturnsPublicContractOnly()
    {
        var listing = CreateListing();
        listing.SubmitForReview(Now);
        listing.Approve(Now.AddMinutes(1));
        using var customizedFactory = CreateFactory(listing);
        using var client = customizedFactory.CreateClient();

        using var response = await client.GetAsync("/api/listings/1");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var json = body.RootElement;
        Assert.Equal("Public listing", json.GetProperty("title").GetString());
        Assert.Equal("House", json.GetProperty("propertyTypeName").GetString());
        Assert.Equal("Public Owner", json.GetProperty("ownerDisplayName").GetString());
        Assert.False(json.TryGetProperty("ownerId", out _));
        Assert.False(json.TryGetProperty("rejectionReason", out _));
    }

    [Fact]
    public async Task GetById_WithUnapprovedListing_ReturnsNotFound()
    {
        using var customizedFactory = CreateFactory(CreateListing());
        using var client = customizedFactory.CreateClient();

        using var response = await client.GetAsync("/api/listings/1");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetById_WhenListingDoesNotExist_ReturnsNotFound()
    {
        using var customizedFactory = CreateFactory(null);
        using var client = customizedFactory.CreateClient();

        using var response = await client.GetAsync("/api/listings/404");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetById_WithNonPositiveId_ReturnsBadRequest()
    {
        using var customizedFactory = CreateFactory(CreateListing());
        using var client = customizedFactory.CreateClient();

        using var response = await client.GetAsync("/api/listings/0");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PublicListingRoute_IsPresentInDevelopmentOpenApiDocument()
    {
        using var customizedFactory = CreateFactory(null);
        using var client = customizedFactory.CreateClient();

        using var response = await client.GetAsync("/openapi/v1.json");
        var document = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("/api/listings/{id}", document, StringComparison.Ordinal);
    }

    private WebApplicationFactory<Program> CreateFactory(Listing? listing)
    {
        var customizedFactory = _factory.WithWebHostBuilder(builder =>
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<IListingRepository>();
                services.AddSingleton<IListingRepository>(
                    new StubListingRepository(listing));
                services.RemoveAll<IPropertyTypeRepository>();
                services.AddSingleton<IPropertyTypeRepository>(
                    new StubPropertyTypeRepository());
            }));

        return customizedFactory;
    }

    private static Listing CreateListing() => Listing.Create(
        "private-owner", 1, "Public listing", "Description", ListingPurpose.Sale,
        1_000_000m, 50m, "Province", "District", "Address", Now);

    private sealed class StubListingRepository(Listing? listing)
        : IListingRepository
    {
        public Task<Listing?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(listing);

        public Task<Listing?> GetPublicByIdAsync(
            int id,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(
                listing?.Status == ListingStatus.Approved ? listing : null);

        public Task<string?> GetOwnerDisplayNameAsync(
            string ownerId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<string?>("Public Owner");

        public Task<ListingSearchPage> SearchApprovedAsync(
            ListingSearchCriteria criteria,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(new ListingSearchPage([], 0));

        public Task<ListingSearchPage> GetForOwnerAsync(
            string ownerId,
            int page,
            int pageSize,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(new ListingSearchPage([], 0));

        public Task<IReadOnlyList<Listing>> GetApprovedByIdsAsync(
            IReadOnlyCollection<int> listingIds,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<Listing>>(
                listing is not null &&
                listing.Status == ListingStatus.Approved &&
                listingIds.Contains(listing.Id)
                    ? [listing]
                    : []);

        public Task<TagListingPage> GetApprovedByTagAsync(
            int tagId,
            int page,
            int pageSize,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(new TagListingPage([], page, pageSize, 0, 0));

        public Task<ListingSearchPage> GetForAdminAsync(
            ListingStatus? status,
            int page,
            int pageSize,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(new ListingSearchPage([], 0));

        public Task<ListingSearchPage> SearchForAdminAsync(
            AdminListingCriteria criteria,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(new ListingSearchPage([], 0));

        public Task<IReadOnlyList<ListingStatusCount>> GetStatusCountsAsync(
            CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<ListingStatusCount>>([]);

        public Task AddAsync(
            Listing newListing,
            CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public Task RemoveAsync(
            Listing oldListing,
            CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public Task SaveChangesAsync(
            CancellationToken cancellationToken = default) =>
            Task.CompletedTask;
    }

    private sealed class StubPropertyTypeRepository
        : IPropertyTypeRepository
    {
        public Task<PropertyType?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<PropertyType?>(PropertyType.Create("House"));

        public Task<IReadOnlyList<PropertyType>> GetActiveAsync(
            CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<PropertyType>>(
                [PropertyType.Create("House")]);

        public Task<IReadOnlyList<PropertyType>> GetAllAsync(
            CancellationToken cancellationToken = default) =>
            GetActiveAsync(cancellationToken);

        public Task AddAsync(
            PropertyType propertyType,
            CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public Task SaveChangesAsync(
            CancellationToken cancellationToken = default) =>
            Task.CompletedTask;
    }
}
