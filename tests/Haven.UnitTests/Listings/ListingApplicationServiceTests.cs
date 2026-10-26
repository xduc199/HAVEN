using Haven.Application.Abstractions;
using Haven.Application.Listings;
using Haven.Domain.Properties;
using Moq;

namespace Haven.UnitTests.Listings;

public sealed class ListingApplicationServiceTests
{
    private static readonly DateTimeOffset Now =
        new(2026, 10, 26, 12, 0, 0, TimeSpan.Zero);

    private readonly Mock<IListingRepository> _listingRepository = new();
    private readonly Mock<IPropertyTypeRepository> _propertyTypeRepository = new();
    private readonly TimeProvider _timeProvider;

    public ListingApplicationServiceTests()
    {
        _timeProvider = new FixedTimeProvider(Now);
    }

    private ListingApplicationService CreateService()
    {
        return new ListingApplicationService(
            _listingRepository.Object,
            _propertyTypeRepository.Object,
            _timeProvider);
    }

    private void SetupActivePropertyType(int id = 1)
    {
        var propertyType = PropertyType.Create("Apartment");

        // PropertyType.Id is assigned by the database, so unit tests
        // should use the same ID that the mocked repository is queried with.
        _propertyTypeRepository
            .Setup(x => x.GetByIdAsync(
                id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => propertyType);
    }

    private static CreateListingCommand CreateCommand(int propertyTypeId = 1)
    {
        return new CreateListingCommand(
            PropertyTypeId: propertyTypeId,
            Title: "Apartment for sale",
            Description: "A test property",
            Purpose: ListingPurpose.Sale,
            PriceAmount: 2_000_000_000m,
            AreaSqm: 80m,
            Province: "Thanh Hoa",
            District: "Thanh Hoa",
            AddressLine: "123 Test Street",
            Bedrooms: 2,
            Bathrooms: 1);
    }

    private static UpdateListingCommand UpdateCommand(int propertyTypeId = 1)
    {
        return new UpdateListingCommand(
            PropertyTypeId: propertyTypeId,
            Title: "Updated apartment",
            Description: "Updated test property",
            Purpose: ListingPurpose.Sale,
            PriceAmount: 2_100_000_000m,
            AreaSqm: 82m,
            Province: "Thanh Hoa",
            District: "Thanh Hoa",
            AddressLine: "123 Test Street",
            Bedrooms: 2,
            Bathrooms: 1);
    }

    [Fact]
    public async Task CreateAsync_WithActivePropertyType_CreatesListing()
    {
        SetupActivePropertyType();

        Listing? savedListing = null;

        _listingRepository
            .Setup(x => x.AddAsync(
                It.IsAny<Listing>(),
                It.IsAny<CancellationToken>()))
            .Callback<Listing, CancellationToken>((listing, _) =>
                savedListing = listing)
            .Returns(Task.CompletedTask);

        var service = CreateService();

        var result = await service.CreateAsync(
            "owner-1",
            CreateCommand());

        Assert.NotNull(result);
        Assert.Equal("owner-1", result.OwnerId);
        Assert.Equal(ListingStatus.Draft, result.Status);
        Assert.Same(result, savedListing);

        _listingRepository.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WithMissingPropertyType_ThrowsArgumentException()
    {
        _propertyTypeRepository
            .Setup(x => x.GetByIdAsync(
                999,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((PropertyType?)null);

        var service = CreateService();

        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.CreateAsync("owner-1", CreateCommand(999)));

        _listingRepository.Verify(
            x => x.AddAsync(
                It.IsAny<Listing>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _listingRepository.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task CreateAsync_WithInactivePropertyType_ThrowsArgumentException()
    {
        var propertyType = PropertyType.Create("Apartment");
        propertyType.Deactivate();

        _propertyTypeRepository
            .Setup(x => x.GetByIdAsync(
                1,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(propertyType);

        var service = CreateService();

        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.CreateAsync("owner-1", CreateCommand()));

        _listingRepository.Verify(
            x => x.AddAsync(
                It.IsAny<Listing>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

[Fact]
public async Task GetOwnedAsync_WithDifferentOwner_ReturnsNull()
{
    const int listingId = 1;
    var listing = CreateListing("owner-1");

    _listingRepository
        .Setup(x => x.GetByIdAsync(
            listingId,
            It.IsAny<CancellationToken>()))
        .ReturnsAsync(listing);

    var service = CreateService();

    var result = await service.GetOwnedAsync(
        "owner-2",
        listingId);

    Assert.Null(result);
}

   [Fact]
public async Task GetOwnedAsync_WithSameOwner_ReturnsListing()
{
    const int listingId = 1;
    var listing = CreateListing("owner-1");

    _listingRepository
        .Setup(x => x.GetByIdAsync(
            listingId,
            It.IsAny<CancellationToken>()))
        .ReturnsAsync(listing);

    var service = CreateService();

    var result = await service.GetOwnedAsync(
        "owner-1",
        listingId);

    Assert.Same(listing, result);
}

    [Fact]
    public async Task UpdateAsync_WithMissingListing_ReturnsNull()
    {
        _listingRepository
            .Setup(x => x.GetByIdAsync(
                42,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Listing?)null);

        var service = CreateService();

        var result = await service.UpdateAsync(
            "owner-1",
            42,
            UpdateCommand());

        Assert.Null(result);

        _propertyTypeRepository.Verify(
            x => x.GetByIdAsync(
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
public async Task UpdateAsync_WithInactivePropertyType_ThrowsArgumentException()
{
    const int listingId = 1;

    var listing = CreateListing("owner-1");

    var propertyType = PropertyType.Create("Apartment");
    propertyType.Deactivate();

    _listingRepository
        .Setup(x => x.GetByIdAsync(
            listingId,
            It.IsAny<CancellationToken>()))
        .ReturnsAsync(listing);

    _propertyTypeRepository
        .Setup(x => x.GetByIdAsync(
            1,
            It.IsAny<CancellationToken>()))
        .ReturnsAsync(propertyType);

    var service = CreateService();

    await Assert.ThrowsAsync<ArgumentException>(() =>
        service.UpdateAsync(
            "owner-1",
            listingId,
            UpdateCommand()));

    _listingRepository.Verify(
        x => x.SaveChangesAsync(
            It.IsAny<CancellationToken>()),
        Times.Never);
}

    private static Listing CreateListing(string ownerId)
    {
        return Listing.Create(
            ownerId: ownerId,
            propertyTypeId: 1,
            title: "Apartment",
            description: "Test listing",
            purpose: ListingPurpose.Sale,
            priceAmount: 2_000_000_000m,
            areaSqm: 80m,
            province: "Thanh Hoa",
            district: "Thanh Hoa",
            addressLine: "123 Test Street",
            nowUtc: Now,
            bedrooms: 2,
            bathrooms: 1);
    }

    private sealed class FixedTimeProvider : TimeProvider
    {
        private readonly DateTimeOffset _now;

        public FixedTimeProvider(DateTimeOffset now)
        {
            _now = now;
        }

        public override DateTimeOffset GetUtcNow() => _now;
    }
}