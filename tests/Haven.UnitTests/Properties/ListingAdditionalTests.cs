using Haven.Domain.Properties;
using Xunit;

namespace Haven.UnitTests.Properties;

public sealed class ListingAdditionalTests
{
    private static readonly DateTimeOffset Now =
        new(2026, 10, 26, 13, 0, 0, TimeSpan.Zero);

    private static Listing CreateListing(
        int? bedrooms = null,
        int? bathrooms = null)
    {
        return Listing.Create(
            ownerId: "user-123",
            propertyTypeId: 1,
            title: "Nhà phố",
            description: "Mô tả",
            purpose: ListingPurpose.Sale,
            priceAmount: 2_000_000_000m,
            areaSqm: 80m,
            province: "Thanh Hóa",
            district: "Đông Sơn",
            addressLine: "Địa chỉ",
            nowUtc: Now,
            bedrooms: bedrooms,
            bathrooms: bathrooms);
    }

    [Fact]
    public void Create_WithNegativeBedrooms_ShouldThrow()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => CreateListing(bedrooms: -1));
    }

    [Fact]
    public void Create_WithNegativeBathrooms_ShouldThrow()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => CreateListing(bathrooms: -1));
    }

    [Fact]
    public void Create_WithNullRoomCounts_ShouldSucceed()
    {
        var listing = CreateListing();

        Assert.Null(listing.Bedrooms);
        Assert.Null(listing.Bathrooms);
    }

    [Fact]
    public void Hide_ApprovedListing_ShouldBecomeHidden()
    {
        var listing = CreateListing();
        listing.SubmitForReview(Now);
        listing.Approve(Now.AddMinutes(1));

        var hiddenAt = Now.AddHours(1);
        listing.Hide(hiddenAt);

        Assert.Equal(ListingStatus.Hidden, listing.Status);
        Assert.Equal(hiddenAt, listing.UpdatedAtUtc);
    }

    [Fact]
    public void Hide_DraftListing_ShouldThrow()
    {
        var listing = CreateListing();

        Assert.Throws<InvalidOperationException>(
            () => listing.Hide(Now));
    }

    [Fact]
    public void UpdateCoreDetails_HiddenListing_ShouldThrow()
    {
        var listing = CreateListing();
        listing.SubmitForReview(Now);
        listing.Approve(Now.AddMinutes(1));
        listing.Hide(Now.AddMinutes(2));

        Assert.Throws<InvalidOperationException>(() =>
            listing.UpdateCoreDetails(
                propertyTypeId: 1,
                title: "Tiêu đề mới",
                description: "Mô tả mới",
                purpose: ListingPurpose.Sale,
                priceAmount: 2_100_000_000m,
                areaSqm: 85m,
                province: "Thanh Hóa",
                district: "Đông Sơn",
                addressLine: "Địa chỉ",
                nowUtc: Now.AddMinutes(3)));
    }
}
