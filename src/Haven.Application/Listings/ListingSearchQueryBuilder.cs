using Haven.Domain.Properties;

namespace Haven.Application.Listings;

/// <summary>
/// Keeps public visibility, filters, and the sort allowlist in one queryable
/// pipeline so EF Core can translate the work to the database.
/// </summary>
public static class ListingSearchQueryBuilder
{
    public const string NewestSort = "newest";
    public const string PriceAscendingSort = "price_asc";
    public const string PriceDescendingSort = "price_desc";

    public static bool IsSupportedSort(string? sortBy)
    {
        return string.IsNullOrWhiteSpace(sortBy) ||
            sortBy.Trim().ToLowerInvariant() is
                NewestSort or PriceAscendingSort or PriceDescendingSort;
    }

    public static IQueryable<Listing> ApplyFilters(
        IQueryable<Listing> source,
        ListingSearchCriteria criteria)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(criteria);

        var query = source.Where(listing => listing.Status == ListingStatus.Approved);

        if (!string.IsNullOrWhiteSpace(criteria.Keyword))
        {
            var keyword = criteria.Keyword.Trim();
            query = query.Where(listing =>
                listing.Title.Contains(keyword) ||
                listing.Description.Contains(keyword) ||
                listing.AddressLine.Contains(keyword));
        }

        if (criteria.PropertyTypeId.HasValue)
        {
            query = query.Where(listing =>
                listing.PropertyTypeId == criteria.PropertyTypeId.Value);
        }

        if (criteria.Purpose.HasValue)
        {
            var purpose = criteria.Purpose.Value;
            query = query.Where(listing => listing.Purpose == purpose);
        }

        if (criteria.MinPriceAmount.HasValue)
        {
            query = query.Where(listing =>
                listing.PriceAmount >= criteria.MinPriceAmount.Value);
        }

        if (criteria.MaxPriceAmount.HasValue)
        {
            query = query.Where(listing =>
                listing.PriceAmount <= criteria.MaxPriceAmount.Value);
        }

        if (criteria.MinAreaSqm.HasValue)
        {
            query = query.Where(listing => listing.AreaSqm >= criteria.MinAreaSqm.Value);
        }

        if (criteria.MaxAreaSqm.HasValue)
        {
            query = query.Where(listing => listing.AreaSqm <= criteria.MaxAreaSqm.Value);
        }

        if (!string.IsNullOrWhiteSpace(criteria.Province))
        {
            var province = criteria.Province.Trim();
            query = query.Where(listing => listing.Province.Contains(province));
        }

        if (!string.IsNullOrWhiteSpace(criteria.District))
        {
            var district = criteria.District.Trim();
            query = query.Where(listing => listing.District.Contains(district));
        }

        return query;
    }

    public static IOrderedQueryable<Listing> ApplySort(
        IQueryable<Listing> source,
        string? sortBy)
    {
        ArgumentNullException.ThrowIfNull(source);

        var normalizedSort = string.IsNullOrWhiteSpace(sortBy)
            ? NewestSort
            : sortBy.Trim().ToLowerInvariant();

        return normalizedSort switch
        {
            NewestSort => source
                .OrderByDescending(listing => listing.CreatedAtUtc)
                .ThenByDescending(listing => listing.Id),
            PriceAscendingSort => source
                .OrderBy(listing => listing.PriceAmount)
                .ThenBy(listing => listing.Id),
            PriceDescendingSort => source
                .OrderByDescending(listing => listing.PriceAmount)
                .ThenBy(listing => listing.Id),
            _ => throw new ListingSearchValidationException(
                $"SortBy must be one of: {NewestSort}, {PriceAscendingSort}, {PriceDescendingSort}.")
        };
    }
}
