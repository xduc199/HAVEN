using Haven.Domain.Properties;

namespace Haven.Application.Favorites;

public sealed record FavoriteListingDetails(
    int ListingId,
    string Title,
    ListingPurpose Purpose,
    decimal PriceAmount,
    decimal AreaSqm,
    string Province,
    string District,
    DateTimeOffset ListingCreatedAtUtc,
    DateTimeOffset FavoritedAtUtc);

public sealed record FavoritePage(
    IReadOnlyList<FavoriteListingDetails> Items,
    int Page,
    int PageSize,
    long TotalCount,
    long TotalPages);

public enum AddFavoriteResult
{
    Added,
    AlreadyExists,
    ListingNotFound
}
