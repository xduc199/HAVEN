using System.ComponentModel.DataAnnotations;
using Haven.Application.Listings;
using Haven.Domain.Properties;
using Haven.Web.Models.Validation;

namespace Haven.Web.Models.Listings;

public sealed class PublicListingSearchRequest
{
    [StringLength(100)]
    public string? Keyword { get; init; }

    [Range(1, int.MaxValue)]
    public int? PropertyTypeId { get; init; }

    [EnumDataType(typeof(ListingPurpose))]
    public ListingPurpose? Purpose { get; init; }

    [StringLength(Listing.ProvinceMaxLength)]
    public string? Province { get; init; }

    [StringLength(Listing.DistrictMaxLength)]
    public string? District { get; init; }

    [DecimalRange("0", "9999999999999999.99")]
    public decimal? MinimumPrice { get; init; }

    [DecimalRange("0", "9999999999999999.99")]
    public decimal? MaximumPrice { get; init; }

    [DecimalRange("0", "99999999.99")]
    public decimal? MinimumAreaSqm { get; init; }

    [DecimalRange("0", "99999999.99")]
    public decimal? MaximumAreaSqm { get; init; }

    [Range(0, int.MaxValue)]
    public int? Bedrooms { get; init; }

    [EnumDataType(typeof(ListingSearchSort))]
    public ListingSearchSort Sort { get; init; } = ListingSearchSort.Newest;

    [Range(1, int.MaxValue)]
    public int Page { get; init; } = 1;

    [Range(1, 100)]
    public int PageSize { get; init; } = 20;
}
