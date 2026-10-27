using Haven.Domain.Properties;

namespace Haven.Application.Listings;

public sealed record PublicListingDetails(
    int Id,
    int PropertyTypeId,
    string Title,
    string Description,
    ListingPurpose Purpose,
    decimal PriceAmount,
    decimal AreaSqm,
    string Province,
    string District,
    string AddressLine,
    int? Bedrooms,
    int? Bathrooms,
    DateTimeOffset CreatedAtUtc);
