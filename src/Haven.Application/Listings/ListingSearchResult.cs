using Haven.Domain.Properties;

namespace Haven.Application.Listings;

public sealed record ListingSearchItem(
    int Id,
    int PropertyTypeId,
    string Title,
    ListingPurpose Purpose,
    decimal PriceAmount,
    decimal AreaSqm,
    string Province,
    string District,
    int? Bedrooms,
    int? Bathrooms,
    DateTimeOffset CreatedAtUtc);

public sealed record ListingSearchResult(
    IReadOnlyList<ListingSearchItem> Items,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages);

public sealed class ListingSearchValidationException(string message)
    : Exception(message);
