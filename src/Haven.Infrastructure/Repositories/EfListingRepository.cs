using Haven.Application.Abstractions;
using Haven.Domain.Properties;
using Haven.Application.Listings;
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

    public async Task<ListingSearchResult> SearchPublicAsync(
        ListingSearchCriteria criteria,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(criteria);

        var filteredQuery = ListingSearchQueryBuilder.ApplyFilters(
            _dbContext.Listings.AsNoTracking(),
            criteria);

        var totalCount = await filteredQuery.CountAsync(cancellationToken);
        var totalPages = totalCount == 0
            ? 0
            : (int)Math.Ceiling(totalCount / (double)criteria.PageSize);
        var offset = (int)(((long)criteria.Page - 1) * criteria.PageSize);

        var items = await ListingSearchQueryBuilder
            .ApplySort(filteredQuery, criteria.SortBy)
            .Skip(offset)
            .Take(criteria.PageSize)
            .Select(listing => new ListingSearchItem(
                listing.Id,
                listing.PropertyTypeId,
                listing.Title,
                listing.Purpose,
                listing.PriceAmount,
                listing.AreaSqm,
                listing.Province,
                listing.District,
                listing.Bedrooms,
                listing.Bathrooms,
                listing.CreatedAtUtc))
            .ToListAsync(cancellationToken);

        return new ListingSearchResult(
            items,
            criteria.Page,
            criteria.PageSize,
            totalCount,
            totalPages);
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
