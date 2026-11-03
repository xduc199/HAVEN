using System.ComponentModel.DataAnnotations;
using Haven.Application.Listings;
using Haven.Domain.Properties;
using Haven.Web.Models.Validation;

namespace Haven.Web.ViewModels.Properties;

public sealed class PropertySearchViewModel
{
    [StringLength(100)]
    public string? Keyword { get; set; }

    [Range(1, int.MaxValue)]
    public int? PropertyTypeId { get; set; }

    [EnumDataType(typeof(ListingPurpose))]
    public ListingPurpose? Purpose { get; set; }

    [StringLength(100)]
    public string? Province { get; set; }

    [StringLength(100)]
    public string? District { get; set; }

    [DecimalRange("0", "9999999999999999.99")]
    public decimal? MinimumPrice { get; set; }

    [DecimalRange("0", "9999999999999999.99")]
    public decimal? MaximumPrice { get; set; }

    [DecimalRange("0", "99999999.99")]
    public decimal? MinimumAreaSqm { get; set; }

    [DecimalRange("0", "99999999.99")]
    public decimal? MaximumAreaSqm { get; set; }

    [Range(0, int.MaxValue)]
    public int? Bedrooms { get; set; }

    [EnumDataType(typeof(ListingSearchSort))]
    public ListingSearchSort Sort { get; set; } = ListingSearchSort.Newest;

    [Range(1, int.MaxValue)]
    public int Page { get; set; } = 1;

    public IReadOnlyList<PropertyTypeOptionViewModel> PropertyTypes { get; set; } = [];
    public IReadOnlyList<PropertyCardViewModel> Items { get; set; } = [];
    public long TotalCount { get; set; }
    public long TotalPages { get; set; }
}

public sealed record PropertyTypeOptionViewModel(int Id, string Name);

public sealed record PropertyCardViewModel(
    int Id,
    string Title,
    ListingPurpose Purpose,
    decimal PriceAmount,
    decimal AreaSqm,
    string PropertyTypeName,
    string Province,
    string District,
    int? Bedrooms);

public sealed record PropertyDetailsViewModel(
    PublicListingDetails Listing,
    bool IsFavorite,
    IReadOnlyList<PropertyTypeOptionViewModel> PropertyTypes);

public sealed record HomePageViewModel(
    IReadOnlyList<PropertyCardViewModel> LatestListings,
    IReadOnlyList<PropertyTypeOptionViewModel> PropertyTypes);

public sealed record ComparePageViewModel(
    IReadOnlyList<PublicListingDetails> Listings,
    IReadOnlyList<int> ListingIds);
