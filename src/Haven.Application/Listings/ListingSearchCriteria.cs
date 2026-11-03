using Haven.Domain.Properties;

namespace Haven.Application.Listings;

public enum ListingSearchSort
{
    Newest = 0,
    Oldest = 1,
    PriceAscending = 2,
    PriceDescending = 3,
    AreaAscending = 4,
    AreaDescending = 5
}

public sealed record ListingSearchCriteria(
    string? Keyword,
    int? PropertyTypeId,
    ListingPurpose? Purpose,
    string? Province,
    string? District,
    decimal? MinimumPrice,
    decimal? MaximumPrice,
    decimal? MinimumAreaSqm,
    decimal? MaximumAreaSqm,
    int? Bedrooms,
    ListingSearchSort Sort,
    int Page,
    int PageSize,
    int? TagId = null);

public sealed record ListingSearchPage(
    IReadOnlyList<Listing> Listings,
    long TotalCount);

public sealed record AdminListingCriteria(
    string? Search,
    ListingStatus? Status,
    int? PropertyTypeId,
    int? TagId,
    DateTimeOffset? FromUtc,
    DateTimeOffset? ToUtcExclusive,
    int Page,
    int PageSize);
