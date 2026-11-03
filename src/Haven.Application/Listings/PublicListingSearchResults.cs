using Haven.Domain.Properties;

namespace Haven.Application.Listings;

public sealed record PublicListingSummary(
    int Id,
    int PropertyTypeId,
    string Title,
    ListingPurpose Purpose,
    decimal PriceAmount,
    decimal AreaSqm,
    string Province,
    string District,
    int? Bedrooms,
    DateTimeOffset CreatedAtUtc);

public sealed record PublicListingSearchResults(
    IReadOnlyList<PublicListingSummary> Items,
    int Page,
    int PageSize,
    long TotalCount,
    long TotalPages);
