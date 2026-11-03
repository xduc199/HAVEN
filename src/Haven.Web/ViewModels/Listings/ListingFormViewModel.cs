using System.ComponentModel.DataAnnotations;
using Haven.Domain.Properties;
using Haven.Web.Models.Validation;
using Haven.Web.ViewModels.Properties;

namespace Haven.Web.ViewModels.Listings;

public sealed class ListingFormViewModel
{
    [Range(1, int.MaxValue)]
    public int PropertyTypeId { get; set; }

    [Required]
    [StringLength(Listing.TitleMaxLength, MinimumLength = 1)]
    public string Title { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = true)]
    [StringLength(Listing.DescriptionMaxLength)]
    public string Description { get; set; } = string.Empty;

    [EnumDataType(typeof(ListingPurpose))]
    public ListingPurpose Purpose { get; set; }

    [DecimalRange("0.01", "9999999999999999.99")]
    public decimal PriceAmount { get; set; }

    [DecimalRange("0.01", "99999999.99")]
    public decimal AreaSqm { get; set; }

    [Required]
    [StringLength(Listing.ProvinceMaxLength, MinimumLength = 1)]
    public string Province { get; set; } = string.Empty;

    [Required]
    [StringLength(Listing.DistrictMaxLength, MinimumLength = 1)]
    public string District { get; set; } = string.Empty;

    [Required]
    [StringLength(Listing.AddressLineMaxLength, MinimumLength = 1)]
    public string AddressLine { get; set; } = string.Empty;

    [Range(0, int.MaxValue)]
    public int? Bedrooms { get; set; }

    [Range(0, int.MaxValue)]
    public int? Bathrooms { get; set; }

    public IReadOnlyList<PropertyTypeOptionViewModel> PropertyTypes { get; set; } = [];
}
