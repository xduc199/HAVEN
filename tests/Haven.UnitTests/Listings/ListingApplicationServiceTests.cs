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
    private readonly ListingApplicationService _service;

    public ListingApplicationServiceTests()
    {
        _service = new ListingApplicationService(
            _listingRepository.Object,
            _propertyTypeRepository.Object,
            new FixedTimeProvider(Now));
    }

    [Fact]
    public async Task CreateAsync_WithActivePropertyType_CreatesDraftListing()
    {
        SetupPropertyType(isActive: true);
        Listing? addedListing = null;
        _listingRepository
            .Setup(x => x.AddAsync(It.IsAny<Listing>(), It.IsAny<CancellationToken>()))
            .Callback<Listing, CancellationToken>((listing, _) => addedListing = listing)
            .Returns(Task.CompletedTask);

        var result = await _service.CreateAsync("owner-1", CreateCommand());

        Assert.Equal("owner-1", result.OwnerId);
        Assert.Equal(ListingStatus.Draft, result.Status);
        Assert.Same(result, addedListing);
        VerifySavedOnce();
    }

    [Fact]
    public async Task CreateAsync_WithMissingPropertyType_DoesNotSave()
    {
        _propertyTypeRepository
            .Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync((PropertyType?)null);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.CreateAsync("owner-1", CreateCommand()));

        _listingRepository.Verify(
            x => x.AddAsync(It.IsAny<Listing>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _listingRepository.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task CreateAsync_WithInactivePropertyType_DoesNotSave()
    {
        SetupPropertyType(isActive: false);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.CreateAsync("owner-1", CreateCommand()));

        _listingRepository.Verify(
            x => x.AddAsync(It.IsAny<Listing>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _listingRepository.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task GetOwnedAsync_WhenOwnerDoesNotMatch_ReturnsNull()
    {
        SetupListing(CreateListing("owner-1"));

        var result = await _service.GetOwnedAsync("owner-2", 1);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetOwnedAsync_WhenOwnerMatches_ReturnsListing()
    {
        var listing = CreateListing("owner-1");
        SetupListing(listing);

        var result = await _service.GetOwnedAsync("owner-1", 1);

        Assert.Same(listing, result);
    }

    [Fact]
    public async Task GetPublicDetailsAsync_WithApprovedListing_ReturnsPublicDto()
    {
        var listing = CreateListing("private-owner");
        listing.SubmitForReview(Now);
        listing.Approve(Now.AddMinutes(1));
        _listingRepository
            .Setup(x => x.GetPublicByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(listing);

        var result = await _service.GetPublicDetailsAsync(1);

        Assert.NotNull(result);
        Assert.Equal("Listing title", result.Title);
        Assert.Equal(ListingPurpose.Sale, result.Purpose);
        Assert.DoesNotContain("OwnerId", typeof(PublicListingDetails)
            .GetProperties().Select(property => property.Name));
        Assert.DoesNotContain("RejectionReason", typeof(PublicListingDetails)
            .GetProperties().Select(property => property.Name));
    }

    [Theory]
    [InlineData(ListingStatus.Draft)]
    [InlineData(ListingStatus.Pending)]
    [InlineData(ListingStatus.Rejected)]
    [InlineData(ListingStatus.Hidden)]
    public async Task GetPublicDetailsAsync_WithNonApprovedListing_ReturnsNull(
        ListingStatus status)
    {
        var listing = CreateListingInStatus(status);
        _listingRepository
            .Setup(x => x.GetPublicByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(listing);

        var result = await _service.GetPublicDetailsAsync(1);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetPublicDetailsAsync_WhenMissing_ReturnsNull()
    {
        _listingRepository
            .Setup(x => x.GetPublicByIdAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Listing?)null);

        var result = await _service.GetPublicDetailsAsync(99);

        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateAsync_WithOwnedListingAndActiveType_UpdatesAndSaves()
    {
        var listing = CreateListing("owner-1");
        SetupListing(listing);
        SetupPropertyType(isActive: true);

        var result = await _service.UpdateAsync(
            "owner-1",
            1,
            UpdateCommand());

        Assert.Same(listing, result);
        Assert.Equal("Updated title", listing.Title);
        Assert.Equal(2_100_000_000m, listing.PriceAmount);
        Assert.Equal(82m, listing.AreaSqm);
        Assert.Equal(ListingStatus.Draft, listing.Status);
        VerifySavedOnce();
    }

    [Fact]
    public async Task UpdateAsync_WithApprovedListing_ReturnsItToPending()
    {
        var listing = CreateListing("owner-1");
        listing.SubmitForReview(Now);
        listing.Approve(Now.AddMinutes(1));
        SetupListing(listing);
        SetupPropertyType(isActive: true);

        await _service.UpdateAsync("owner-1", 1, UpdateCommand());

        Assert.Equal(ListingStatus.Pending, listing.Status);
        Assert.Null(listing.ApprovedAtUtc);
        Assert.Equal(Now, listing.SubmittedAtUtc);
        VerifySavedOnce();
    }

    [Fact]
    public async Task UpdateAsync_WhenOwnerDoesNotMatch_DoesNotCheckTypeOrSave()
    {
        SetupListing(CreateListing("owner-1"));

        var result = await _service.UpdateAsync(
            "owner-2",
            1,
            UpdateCommand());

        Assert.Null(result);
        _propertyTypeRepository.Verify(
            x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _listingRepository.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_WithInactivePropertyType_DoesNotSave()
    {
        SetupListing(CreateListing("owner-1"));
        SetupPropertyType(isActive: false);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.UpdateAsync("owner-1", 1, UpdateCommand()));

        _listingRepository.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task SubmitForReviewAsync_WithOwnedDraft_SetsPendingAndSaves()
    {
        var listing = CreateListing("owner-1");
        SetupListing(listing);

        var result = await _service.SubmitForReviewAsync("owner-1", 1);

        Assert.Same(listing, result);
        Assert.Equal(ListingStatus.Pending, listing.Status);
        Assert.Equal(Now, listing.SubmittedAtUtc);
        VerifySavedOnce();
    }

    [Fact]
    public async Task SubmitForReviewAsync_WhenOwnerDoesNotMatch_DoesNotSave()
    {
        SetupListing(CreateListing("owner-1"));

        var result = await _service.SubmitForReviewAsync("owner-2", 1);

        Assert.Null(result);
        _listingRepository.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ApproveAsync_WithPendingListing_ApprovesAndSaves()
    {
        var listing = CreateListingInStatus(ListingStatus.Pending);
        SetupListing(listing);

        var result = await _service.ApproveAsync(1);

        Assert.Same(listing, result);
        Assert.Equal(ListingStatus.Approved, listing.Status);
        Assert.Equal(Now, listing.ApprovedAtUtc);
        VerifySavedOnce();
    }

    [Fact]
    public async Task RejectAsync_WithPendingListing_StoresReasonAndSaves()
    {
        var listing = CreateListingInStatus(ListingStatus.Pending);
        SetupListing(listing);

        var result = await _service.RejectAsync(1, "  Missing documents  ");

        Assert.Same(listing, result);
        Assert.Equal(ListingStatus.Rejected, listing.Status);
        Assert.Equal("Missing documents", listing.RejectionReason);
        VerifySavedOnce();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task RejectAsync_WithBlankReason_DoesNotSave(string reason)
    {
        SetupListing(CreateListingInStatus(ListingStatus.Pending));

        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.RejectAsync(1, reason));

        _listingRepository.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task HideAsync_WithApprovedListing_HidesAndSaves()
    {
        var listing = CreateListingInStatus(ListingStatus.Approved);
        SetupListing(listing);

        var result = await _service.HideAsync(1);

        Assert.Same(listing, result);
        Assert.Equal(ListingStatus.Hidden, listing.Status);
        VerifySavedOnce();
    }

    [Theory]
    [InlineData(nameof(ListingApplicationService.ApproveAsync), ListingStatus.Draft)]
    [InlineData(nameof(ListingApplicationService.RejectAsync), ListingStatus.Approved)]
    [InlineData(nameof(ListingApplicationService.HideAsync), ListingStatus.Pending)]
    public async Task Moderation_WithInvalidTransition_DoesNotSave(
        string operation,
        ListingStatus status)
    {
        SetupListing(CreateListingInStatus(status));

        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            switch (operation)
            {
                case nameof(ListingApplicationService.ApproveAsync):
                    await _service.ApproveAsync(1);
                    break;
                case nameof(ListingApplicationService.RejectAsync):
                    await _service.RejectAsync(1, "Rejected");
                    break;
                default:
                    await _service.HideAsync(1);
                    break;
            }
        });

        _listingRepository.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Moderation_WhenListingDoesNotExist_ReturnsNull()
    {
        _listingRepository
            .Setup(x => x.GetByIdAsync(404, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Listing?)null);

        Assert.Null(await _service.ApproveAsync(404));
        Assert.Null(await _service.RejectAsync(404, "reason"));
        Assert.Null(await _service.HideAsync(404));
        _listingRepository.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private void SetupListing(Listing listing, int id = 1)
    {
        _listingRepository
            .Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(listing);
    }

    private void SetupPropertyType(bool isActive)
    {
        var propertyType = PropertyType.Create("Apartment");
        if (!isActive)
        {
            propertyType.Deactivate();
        }

        _propertyTypeRepository
            .Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(propertyType);
    }

    private void VerifySavedOnce()
    {
        _listingRepository.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private static CreateListingCommand CreateCommand() => new(
        1, "Listing title", "Description", ListingPurpose.Sale,
        2_000_000_000m, 80m, "Province", "District", "Address");

    private static UpdateListingCommand UpdateCommand() => new(
        1, "Updated title", "Updated description", ListingPurpose.Sale,
        2_100_000_000m, 82m, "Updated province", "Updated district", "Updated address");

    private static Listing CreateListing(string ownerId) => Listing.Create(
        ownerId, 1, "Listing title", "Description", ListingPurpose.Sale,
        2_000_000_000m, 80m, "Province", "District", "Address", Now);

    private static Listing CreateListingInStatus(ListingStatus status)
    {
        var listing = CreateListing("owner-1");
        if (status == ListingStatus.Draft)
        {
            return listing;
        }

        listing.SubmitForReview(Now);
        if (status == ListingStatus.Pending)
        {
            return listing;
        }

        if (status == ListingStatus.Rejected)
        {
            listing.Reject("Rejected", Now.AddMinutes(1));
            return listing;
        }

        listing.Approve(Now.AddMinutes(1));
        if (status == ListingStatus.Hidden)
        {
            listing.Hide(Now.AddMinutes(2));
        }

        return listing;
    }

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}
