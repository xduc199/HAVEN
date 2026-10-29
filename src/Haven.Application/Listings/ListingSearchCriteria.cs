namespace Haven.Application.Listings;

using Haven.Domain.Properties;

/// <summary>
/// Query-string contract for public listing search.
/// Filter semantics are documented in docs/property-search-contract.md.
/// </summary>
public sealed record ListingSearchCriteria
{
    public const int DefaultPageSize = 20;
    public const int MaximumPageSize = 100;
    public const int MaximumKeywordLength = 200;

    public string? Keyword { get; init; }
    public int? PropertyTypeId { get; init; }
    public ListingPurpose? Purpose { get; init; }
    public decimal? MinPriceAmount { get; init; }
    public decimal? MaxPriceAmount { get; init; }
    public decimal? MinAreaSqm { get; init; }
    public decimal? MaxAreaSqm { get; init; }
    public string? Province { get; init; }
    public string? District { get; init; }
    public string? SortBy { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = DefaultPageSize;
}
