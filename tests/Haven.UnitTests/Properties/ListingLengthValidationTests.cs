using Haven.Domain.Properties;

namespace Haven.UnitTests.Properties;

public sealed class ListingLengthValidationTests
{
    private static readonly DateTimeOffset Now =
        new(2026, 10, 26, 13, 0, 0, TimeSpan.Zero);

    public static TheoryData<string, int> ListingFieldLimits => new()
    {
        { "ownerId", Listing.OwnerIdMaxLength },
        { "title", Listing.TitleMaxLength },
        { "description", Listing.DescriptionMaxLength },
        { "province", Listing.ProvinceMaxLength },
        { "district", Listing.DistrictMaxLength },
        { "addressLine", Listing.AddressLineMaxLength }
    };

    [Theory]
    [MemberData(nameof(ListingFieldLimits))]
    public void Create_WithFieldAtMaximumLength_Succeeds(
        string fieldName,
        int maxLength)
    {
        var listing = CreateListingWithField(fieldName, new string('x', maxLength));

        Assert.NotNull(listing);
    }

    [Theory]
    [MemberData(nameof(ListingFieldLimits))]
    public void Create_WithFieldOverMaximumLength_ThrowsArgumentException(
        string fieldName,
        int maxLength)
    {
        Assert.Throws<ArgumentException>(() =>
            CreateListingWithField(fieldName, new string('x', maxLength + 1)));
    }

    [Fact]
    public void Reject_WithReasonAtMaximumLength_Succeeds()
    {
        var listing = CreateListing();
        listing.SubmitForReview(Now);

        listing.Reject(new string('x', Listing.RejectionReasonMaxLength), Now);

        Assert.Equal(ListingStatus.Rejected, listing.Status);
        Assert.Equal(Listing.RejectionReasonMaxLength, listing.RejectionReason?.Length);
    }

    [Fact]
    public void Reject_WithReasonOverMaximumLength_ThrowsBeforeChangingState()
    {
        var listing = CreateListing();
        listing.SubmitForReview(Now);

        Assert.Throws<ArgumentException>(() =>
            listing.Reject(
                new string('x', Listing.RejectionReasonMaxLength + 1),
                Now));

        Assert.Equal(ListingStatus.Pending, listing.Status);
        Assert.Null(listing.RejectionReason);
    }

    private static Listing CreateListingWithField(string fieldName, string value) =>
        Listing.Create(
            ownerId: fieldName == "ownerId" ? value : "owner-1",
            propertyTypeId: 1,
            title: fieldName == "title" ? value : "Title",
            description: fieldName == "description" ? value : "Description",
            purpose: ListingPurpose.Sale,
            priceAmount: 1_000_000m,
            areaSqm: 50m,
            province: fieldName == "province" ? value : "Province",
            district: fieldName == "district" ? value : "District",
            addressLine: fieldName == "addressLine" ? value : "Address",
            nowUtc: Now);

    private static Listing CreateListing() => Listing.Create(
        "owner-1", 1, "Title", "Description", ListingPurpose.Sale,
        1_000_000m, 50m, "Province", "District", "Address", Now);
}
