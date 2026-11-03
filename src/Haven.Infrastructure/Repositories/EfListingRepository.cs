using Haven.Application.Abstractions;
using Haven.Application.Listings;
using Haven.Application.Tags;
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

    public Task<string?> GetOwnerDisplayNameAsync(
        string ownerId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.Users
            .AsNoTracking()
            .Where(user => user.Id == ownerId)
            .Select(user => user.DisplayName)
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<ListingSearchPage> SearchApprovedAsync(
        ListingSearchCriteria criteria,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(criteria);

        IQueryable<Listing> query = _dbContext.Listings
            .AsNoTracking()
            .Where(listing => listing.Status == ListingStatus.Approved);

        if (!string.IsNullOrWhiteSpace(criteria.Keyword))
        {
            var keyword = criteria.Keyword;
            query = query.Where(listing =>
                listing.Title.Contains(keyword) ||
                listing.Description.Contains(keyword) ||
                listing.Province.Contains(keyword) ||
                listing.District.Contains(keyword) ||
                listing.AddressLine.Contains(keyword));
        }

        if (criteria.PropertyTypeId is { } propertyTypeId)
        {
            query = query.Where(
                listing => listing.PropertyTypeId == propertyTypeId);
        }

        if (criteria.Purpose is { } purpose)
        {
            query = query.Where(listing => listing.Purpose == purpose);
        }

        if (!string.IsNullOrWhiteSpace(criteria.Province))
        {
            query = query.Where(
                listing => listing.Province == criteria.Province);
        }

        if (!string.IsNullOrWhiteSpace(criteria.District))
        {
            query = query.Where(
                listing => listing.District == criteria.District);
        }

        if (criteria.MinimumPrice is { } minimumPrice)
        {
            query = query.Where(
                listing => listing.PriceAmount >= minimumPrice);
        }

        if (criteria.MaximumPrice is { } maximumPrice)
        {
            query = query.Where(
                listing => listing.PriceAmount <= maximumPrice);
        }

        if (criteria.MinimumAreaSqm is { } minimumArea)
        {
            query = query.Where(
                listing => listing.AreaSqm >= minimumArea);
        }

        if (criteria.MaximumAreaSqm is { } maximumArea)
        {
            query = query.Where(
                listing => listing.AreaSqm <= maximumArea);
        }

        if (criteria.Bedrooms is { } bedrooms)
        {
            query = query.Where(listing => listing.Bedrooms == bedrooms);
        }

        if (criteria.TagId is { } tagId)
        {
            query = query.Where(listing =>
                listing.ListingTags.Any(link => link.TagId == tagId && link.Tag.IsActive));
        }

        var totalCount = await query.LongCountAsync(cancellationToken);
        query = ApplySort(query, criteria.Sort);

        var listings = await query
            .Skip((criteria.Page - 1) * criteria.PageSize)
            .Take(criteria.PageSize)
            .ToListAsync(cancellationToken);

        return new ListingSearchPage(listings, totalCount);
    }

    public async Task<TagListingPage> GetApprovedByTagAsync(
        int tagId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Listings
            .AsNoTracking()
            .Where(listing =>
                listing.Status == ListingStatus.Approved &&
                listing.ListingTags.Any(link =>
                    link.TagId == tagId && link.Tag.IsActive));
        var totalCount = await query.LongCountAsync(cancellationToken);
        var listings = await query
            .OrderByDescending(listing => listing.CreatedAtUtc)
            .ThenBy(listing => listing.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
        return new TagListingPage(
            listings,
            page,
            pageSize,
            totalCount,
            totalCount == 0 ? 0 : (totalCount + pageSize - 1) / pageSize);
    }

    public async Task<ListingSearchPage> GetForOwnerAsync(
        string ownerId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(ownerId);

        var query = _dbContext.Listings
            .AsNoTracking()
            .Where(listing => listing.OwnerId == ownerId);
        var totalCount = await query.LongCountAsync(cancellationToken);
        var listings = await query
            .OrderByDescending(listing => listing.UpdatedAtUtc)
            .ThenBy(listing => listing.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new ListingSearchPage(listings, totalCount);
    }

    public async Task<IReadOnlyList<Listing>> GetApprovedByIdsAsync(
        IReadOnlyCollection<int> listingIds,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(listingIds);

        return await _dbContext.Listings
            .AsNoTracking()
            .Where(listing =>
                listingIds.Contains(listing.Id) &&
                listing.Status == ListingStatus.Approved)
            .ToListAsync(cancellationToken);
    }

    public async Task<ListingSearchPage> GetForAdminAsync(
        ListingStatus? status,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Listing> query = _dbContext.Listings.AsNoTracking();
        if (status is { } listingStatus)
        {
            query = query.Where(listing => listing.Status == listingStatus);
        }

        var totalCount = await query.LongCountAsync(cancellationToken);
        if (status == ListingStatus.Pending)
        {
            query = query
                .OrderBy(listing => listing.SubmittedAtUtc)
                .ThenBy(listing => listing.Id);
        }
        else
        {
            query = query
                .OrderByDescending(listing => listing.CreatedAtUtc)
                .ThenBy(listing => listing.Id);
        }

        var listings = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
        return new ListingSearchPage(listings, totalCount);
    }

    public async Task<ListingSearchPage> SearchForAdminAsync(
        AdminListingCriteria criteria,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Listing> query = _dbContext.Listings
            .AsNoTracking()
            .Include(listing => listing.ListingTags);
        if (!string.IsNullOrWhiteSpace(criteria.Search))
        {
            var search = criteria.Search.Trim();
            query = query.Where(listing =>
                listing.Title.Contains(search) ||
                listing.Province.Contains(search) ||
                listing.District.Contains(search) ||
                listing.AddressLine.Contains(search));
        }

        if (criteria.Status is { } status)
        {
            query = query.Where(listing => listing.Status == status);
        }

        if (criteria.PropertyTypeId is { } propertyTypeId)
        {
            query = query.Where(listing => listing.PropertyTypeId == propertyTypeId);
        }

        if (criteria.TagId is { } tagId)
        {
            query = query.Where(listing =>
                listing.ListingTags.Any(link => link.TagId == tagId));
        }

        if (criteria.FromUtc is { } from)
        {
            query = query.Where(listing => listing.CreatedAtUtc >= from);
        }

        if (criteria.ToUtcExclusive is { } to)
        {
            query = query.Where(listing => listing.CreatedAtUtc < to);
        }

        var totalCount = await query.LongCountAsync(cancellationToken);
        var listings = await query
            .OrderByDescending(listing => listing.CreatedAtUtc)
            .ThenBy(listing => listing.Id)
            .Skip((criteria.Page - 1) * criteria.PageSize)
            .Take(criteria.PageSize)
            .ToListAsync(cancellationToken);
        return new ListingSearchPage(listings, totalCount);
    }

    public async Task<IReadOnlyList<ListingStatusCount>> GetStatusCountsAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Listings
            .AsNoTracking()
            .GroupBy(listing => listing.Status)
            .Select(group => new ListingStatusCount(
                group.Key,
                group.LongCount()))
            .ToListAsync(cancellationToken);
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

    public Task RemoveAsync(
        Listing listing,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(listing);
        _dbContext.Listings.Remove(listing);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }

    private static IOrderedQueryable<Listing> ApplySort(
        IQueryable<Listing> query,
        ListingSearchSort sort) =>
        sort switch
        {
            ListingSearchSort.Oldest => query
                .OrderBy(listing => listing.CreatedAtUtc)
                .ThenBy(listing => listing.Id),
            ListingSearchSort.PriceAscending => query
                .OrderBy(listing => listing.PriceAmount)
                .ThenBy(listing => listing.Id),
            ListingSearchSort.PriceDescending => query
                .OrderByDescending(listing => listing.PriceAmount)
                .ThenBy(listing => listing.Id),
            ListingSearchSort.AreaAscending => query
                .OrderBy(listing => listing.AreaSqm)
                .ThenBy(listing => listing.Id),
            ListingSearchSort.AreaDescending => query
                .OrderByDescending(listing => listing.AreaSqm)
                .ThenBy(listing => listing.Id),
            _ => query
                .OrderByDescending(listing => listing.CreatedAtUtc)
                .ThenBy(listing => listing.Id)
        };
}
