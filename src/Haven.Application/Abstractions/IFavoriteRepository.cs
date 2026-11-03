using Haven.Application.Favorites;
using Haven.Domain.Properties;

namespace Haven.Application.Abstractions;

public interface IFavoriteRepository
{
    Task<bool> IsApprovedListingAsync(
        int listingId,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(
        string userId,
        int listingId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<FavoriteListingDetails>> GetForUserAsync(
        string userId,
        int skip,
        int take,
        CancellationToken cancellationToken = default);

    Task<long> CountForUserAsync(
        string userId,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        Favorite favorite,
        CancellationToken cancellationToken = default);

    Task<bool> RemoveAsync(
        string userId,
        int listingId,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
