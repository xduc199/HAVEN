using System.ComponentModel.DataAnnotations;
using Haven.Domain.Consultations;
using Haven.Domain.Properties;

namespace Haven.Web.ViewModels.Consultations;

public sealed class CreateConsultationViewModel
{
    [Required]
    [StringLength(ConsultationRequest.NameMaxLength, MinimumLength = 1)]
    public string ContactName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(ConsultationRequest.EmailMaxLength)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(ConsultationRequest.PhoneNumberMaxLength)]
    [RegularExpression(@"^\+?[0-9\s().-]+$")]
    public string PhoneNumber { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int? ListingId { get; set; }

    [StringLength(ConsultationRequest.MessageMaxLength)]
    public string? Message { get; set; }

    public string? ListingTitle { get; set; }
}

public sealed class UpdateConsultationStatusViewModel
{
    [EnumDataType(typeof(ConsultationStatus))]
    public ConsultationStatus Status { get; set; }
}
