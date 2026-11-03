using System.ComponentModel.DataAnnotations;
using Haven.Domain.Properties;
using Haven.Domain.Consultations;

namespace Haven.Web.ViewModels.Admin;

public sealed class PropertyTypeFormViewModel
{
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string Name { get; set; } = string.Empty;

    [Range(0, int.MaxValue)]
    public int SortOrder { get; set; }

    public bool IsActive { get; set; } = true;
}

public sealed class AdminListingFilterViewModel
{
    [StringLength(200)]
    public string? Search { get; set; }

    [EnumDataType(typeof(ListingStatus))]
    public ListingStatus? Status { get; set; }

    [Range(1, int.MaxValue)]
    public int? PropertyTypeId { get; set; }

    [Range(1, int.MaxValue)]
    public int? TagId { get; set; }

    public DateOnly? FromDate { get; set; }

    public DateOnly? ToDate { get; set; }

    [Range(1, int.MaxValue)]
    public int Page { get; set; } = 1;
}

public sealed class AdminTagPageViewModel
{
    public required Haven.Application.Tags.TagPage Page { get; init; }
    public string? Search { get; init; }
}

public sealed class TagFormViewModel
{
    [Required]
    [StringLength(Tag.NameMaxLength, MinimumLength = 1)]
    public string Name { get; set; } = string.Empty;

    [StringLength(Tag.DescriptionMaxLength)]
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;
}

public sealed class ListingTagAssignmentViewModel
{
    [Range(1, int.MaxValue)]
    public int TagId { get; set; }
}

public sealed class RejectListingViewModel
{
    [Required]
    [StringLength(Listing.RejectionReasonMaxLength, MinimumLength = 1)]
    public string Reason { get; set; } = string.Empty;
}

public sealed class AdminConsultationFilterViewModel
{
    [StringLength(200)]
    public string? Search { get; set; }

    [EnumDataType(typeof(ConsultationStatus))]
    public ConsultationStatus? Status { get; set; }

    public DateOnly? FromDate { get; set; }

    public DateOnly? ToDate { get; set; }

    [Range(1, int.MaxValue)]
    public int Page { get; set; } = 1;
}
