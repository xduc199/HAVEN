using System.ComponentModel.DataAnnotations;
using Haven.Domain.Properties;
using Haven.Web.Models.Validation;

namespace Haven.Web.Models.Listings;

public sealed record ListingWriteRequest(
    [Range(1, int.MaxValue)]
    int PropertyTypeId,
    [Required]
    [StringLength(Listing.TitleMaxLength, MinimumLength = 1)]
    string Title,
    [Required(AllowEmptyStrings = true)]
    [StringLength(Listing.DescriptionMaxLength)]
    string Description,
    [EnumDataType(typeof(ListingPurpose))]
    ListingPurpose Purpose,
    [DecimalRange("0.01", "9999999999999999.99")]
    decimal PriceAmount,
    [DecimalRange("0.01", "99999999.99")]
    decimal AreaSqm,
    [Required]
    [StringLength(Listing.ProvinceMaxLength, MinimumLength = 1)]
    string Province,
    [Required]
    [StringLength(Listing.DistrictMaxLength, MinimumLength = 1)]
    string District,
    [Required]
    [StringLength(Listing.AddressLineMaxLength, MinimumLength = 1)]
    string AddressLine,
    [Range(0, int.MaxValue)]
    int? Bedrooms = null,
    [Range(0, int.MaxValue)]
    int? Bathrooms = null);
