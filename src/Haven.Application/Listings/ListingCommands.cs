using Haven.Domain.Properties;

namespace Haven.Application.Listings;

public sealed record CreateListingCommand(
    int PropertyTypeId,
    string Title,
    string Description,
    ListingPurpose Purpose,
    decimal PriceAmount,
    decimal AreaSqm,
    string Province,
    string District,
    string AddressLine,
    int? Bedrooms = null,
    int? Bathrooms = null);

public sealed record UpdateListingCommand(
    int PropertyTypeId,
    string Title,
    string Description,
    ListingPurpose Purpose,
    decimal PriceAmount,
    decimal AreaSqm,
    string Province,
    string District,
    string AddressLine,
    int? Bedrooms = null,
    int? Bathrooms = null);
