using Haven.Application.Abstractions;
using Haven.Application.Favorites;
using Haven.Domain.Properties;
using Haven.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Haven.Infrastructure.Repositories;

public sealed class EfFavoriteRepository : IFavoriteRepository
{
    private readonly ApplicationDbContext _dbContext;

    public EfFavoriteRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<bool> IsApprovedListingAsync(
        int listingId,
        CancellationToken cancellationToken = default) =>
        _dbContext.Listings.AnyAsync(
            listing => listing.Id == listingId &&
                listing.Status == ListingStatus.Approved,
            cancellationToken);

    public Task<bool> ExistsAsync(
        string userId,
        int listingId,
        CancellationToken cancellationToken = default) =>
        _dbContext.Favorites.AnyAsync(
            favorite => favorite.UserId == userId &&
                favorite.ListingId == listingId,
            cancellationToken);

    public async Task<IReadOnlyList<FavoriteListingDetails>> GetForUserAsync(
        string userId,
        int skip,
        int take,
        CancellationToken cancellationToken = default)
    {
        var query =
            from favorite in _dbContext.Favorites.AsNoTracking()
            join listing in _dbContext.Listings.AsNoTracking()
                on favorite.ListingId equals listing.Id
            where favorite.UserId == userId &&
                listing.Status == ListingStatus.Approved
            orderby favorite.CreatedAtUtc descending, favorite.ListingId
            select new FavoriteListingDetails(
                listing.Id,
                listing.Title,
                listing.Purpose,
                listing.PriceAmount,
                listing.AreaSqm,
                listing.Province,
                listing.District,
                listing.CreatedAtUtc,
                favorite.CreatedAtUtc);

        return await query
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public Task<long> CountForUserAsync(
        string userId,
        CancellationToken cancellationToken = default) =>
        (from favorite in _dbContext.Favorites.AsNoTracking()
         join listing in _dbContext.Listings.AsNoTracking()
             on favorite.ListingId equals listing.Id
         where favorite.UserId == userId &&
             listing.Status == ListingStatus.Approved
         select favorite)
        .LongCountAsync(cancellationToken);

    public async Task AddAsync(
        Favorite favorite,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(favorite);
        await _dbContext.Favorites.AddAsync(favorite, cancellationToken);
    }

    public async Task<bool> RemoveAsync(
        string userId,
        int listingId,
        CancellationToken cancellationToken = default)
    {
        var favorite = await _dbContext.Favorites.FirstOrDefaultAsync(
            item => item.UserId == userId &&
                item.ListingId == listingId,
            cancellationToken);
        if (favorite is null)
        {
            return false;
        }

        _dbContext.Favorites.Remove(favorite);
        return true;
    }

    public Task SaveChangesAsync(
        CancellationToken cancellationToken = default) =>
        _dbContext.SaveChangesAsync(cancellationToken);
}
