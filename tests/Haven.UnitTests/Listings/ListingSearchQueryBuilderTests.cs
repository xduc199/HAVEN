using Haven.Application.Listings;
using Haven.Domain.Properties;

namespace Haven.UnitTests.Listings;

public sealed class ListingSearchQueryBuilderTests
{
    private static readonly DateTimeOffset BaseTime =
        new(2026, 10, 20, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void ApplyFilters_CombinesApprovedStatusAndProvidedFilters()
    {
        var matches = CreateApprovedListing(
            id: 3,
            title: "Riverside home",
            description: "Quiet garden",
            purpose: ListingPurpose.Rent,
            price: 1_000,
            area: 50,
            province: "Da Nang",
            district: "Ngu Hanh Son",
            propertyTypeId: 2);
        var wrongPrice = CreateApprovedListing(
            id: 4, "Riverside home", "Quiet garden", ListingPurpose.Rent,
            price: 1_001, area: 50, "Da Nang", "Ngu Hanh Son", propertyTypeId: 2);
        var wrongPurpose = CreateApprovedListing(
            id: 5, "Riverside home", "Quiet garden", ListingPurpose.Sale,
            price: 1_000, area: 50, "Da Nang", "Ngu Hanh Son", propertyTypeId: 2);
        var unapproved = CreateListing(
            id: 6, "Riverside home", "Quiet garden", ListingPurpose.Rent,
            price: 1_000, area: 50, "Da Nang", "Ngu Hanh Son", propertyTypeId: 2);

        var criteria = new ListingSearchCriteria
        {
            Keyword = "Riverside",
            PropertyTypeId = 2,
            Purpose = ListingPurpose.Rent,
            MinPriceAmount = 1_000,
            MaxPriceAmount = 1_000,
            MinAreaSqm = 50,
            MaxAreaSqm = 50,
            Province = "Da Nang",
            District = "Ngu Hanh"
        };

        var result = ListingSearchQueryBuilder
            .ApplyFilters(
                new[] { matches, wrongPrice, wrongPurpose, unapproved }.AsQueryable(),
                criteria)
            .ToArray();

        Assert.Equal(new[] { matches }, result);
    }

    [Fact]
    public void ApplyFilters_KeywordSearchesTitleDescriptionAndAddress()
    {
        var titleMatch = CreateApprovedListing(
            1, "Harbor apartment", "Bright rooms", ListingPurpose.Sale,
            2_000, 80, "Da Nang", "Son Tra");
        var descriptionMatch = CreateApprovedListing(
            2, "City apartment", "Near Harbor promenade", ListingPurpose.Sale,
            2_000, 80, "Da Nang", "Son Tra");
        var addressMatch = CreateApprovedListing(
            3, "City apartment", "Bright rooms", ListingPurpose.Sale,
            2_000, 80, "Da Nang", "Son Tra", addressLine: "12 Harbor Road");
        var noMatch = CreateApprovedListing(
            4, "City apartment", "Bright rooms", ListingPurpose.Sale,
            2_000, 80, "Da Nang", "Son Tra", addressLine: "12 Palm Road");

        var result = ListingSearchQueryBuilder.ApplyFilters(
                new[] { titleMatch, descriptionMatch, addressMatch, noMatch }.AsQueryable(),
                new ListingSearchCriteria { Keyword = "Harbor" })
            .Select(listing => listing.Id)
            .Order()
            .ToArray();

        Assert.Equal(new[] { 1, 2, 3 }, result);
    }

    [Fact]
    public void ApplySort_UsesAllowlistedSortsAndStableIdTieBreak()
    {
        var olderHigh = CreateApprovedListing(
            8, "Home", "", ListingPurpose.Sale, 200, 80, "A", "A", createdAt: BaseTime);
        var samePriceLowerId = CreateApprovedListing(
            2, "Home", "", ListingPurpose.Sale, 200, 80, "A", "A", createdAt: BaseTime.AddDays(1));
        var lowPrice = CreateApprovedListing(
            1, "Home", "", ListingPurpose.Sale, 100, 80, "A", "A", createdAt: BaseTime.AddDays(2));
        var samePriceHigherId = CreateApprovedListing(
            5, "Home", "", ListingPurpose.Sale, 200, 80, "A", "A", createdAt: BaseTime.AddDays(1));
        var source = new[] { olderHigh, samePriceHigherId, lowPrice, samePriceLowerId }.AsQueryable();

        var ascending = ListingSearchQueryBuilder.ApplySort(source, "price_asc")
            .Select(listing => listing.Id)
            .ToArray();
        var descending = ListingSearchQueryBuilder.ApplySort(source, "price_desc")
            .Select(listing => listing.Id)
            .ToArray();
        var newest = ListingSearchQueryBuilder.ApplySort(source, null)
            .Select(listing => listing.Id)
            .ToArray();

        Assert.Equal(new[] { 1, 2, 5, 8 }, ascending);
        Assert.Equal(new[] { 2, 5, 8, 1 }, descending);
        Assert.Equal(new[] { 1, 5, 2, 8 }, newest);
        Assert.Equal(new[] { 2, 5 }, ListingSearchQueryBuilder.ApplySort(source, "price_asc")
            .Skip(1).Take(2).Select(listing => listing.Id));
    }

    [Fact]
    public void IsSupportedSort_RejectsUnlistedSortValues()
    {
        Assert.True(ListingSearchQueryBuilder.IsSupportedSort(" PRICE_ASC "));
        Assert.False(ListingSearchQueryBuilder.IsSupportedSort("Title desc"));
    }

    private static Listing CreateApprovedListing(
        int id,
        string title,
        string description,
        ListingPurpose purpose,
        decimal price,
        decimal area,
        string province,
        string district,
        int propertyTypeId = 1,
        string addressLine = "123 Main Street",
        DateTimeOffset? createdAt = null)
    {
        var listing = CreateListing(
            id, title, description, purpose, price, area, province, district,
            propertyTypeId, addressLine, createdAt);
        listing.SubmitForReview(BaseTime.AddHours(1));
        listing.Approve(BaseTime.AddHours(2));
        return listing;
    }

    private static Listing CreateListing(
        int id,
        string title,
        string description,
        ListingPurpose purpose,
        decimal price,
        decimal area,
        string province,
        string district,
        int propertyTypeId = 1,
        string addressLine = "123 Main Street",
        DateTimeOffset? createdAt = null)
    {
        var listing = Listing.Create(
            ownerId: "owner-1",
            propertyTypeId: propertyTypeId,
            title: title,
            description: description,
            purpose: purpose,
            priceAmount: price,
            areaSqm: area,
            province: province,
            district: district,
            addressLine: addressLine,
            nowUtc: createdAt ?? BaseTime);

        // Listing IDs are database-generated and have private setters. Assign a
        // deterministic identity here so the stable-sort tie-break can be tested.
        typeof(Listing).GetProperty(nameof(Listing.Id))!.SetValue(listing, id);
        return listing;
    }
}
