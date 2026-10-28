using Haven.Application.Abstractions;
using Haven.Domain.Properties;
using Haven.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Haven.Infrastructure.Repositories;

public sealed class EfListingRepository : IListingRepository
{
    private readonly ApplicationDbContext _dbContext;

    public EfListingRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Listing?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.Listings
            .FirstOrDefaultAsync(
                listing => listing.Id == id,
                cancellationToken);
    }

    public Task<Listing?> GetPublicByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.Listings
            .FirstOrDefaultAsync(
                listing => listing.Id == id &&
                    listing.Status == ListingStatus.Approved,
                cancellationToken);
    }

    public async Task AddAsync(
        Listing listing,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(listing);

        await _dbContext.Listings.AddAsync(
            listing,
            cancellationToken);
    }

    public Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
