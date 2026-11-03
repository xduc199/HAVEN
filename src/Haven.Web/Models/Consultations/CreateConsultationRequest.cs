using System.ComponentModel.DataAnnotations;
using Haven.Domain.Consultations;

namespace Haven.Web.Models.Consultations;

public sealed record CreateConsultationRequest(
    [param: Required]
    [param: StringLength(ConsultationRequest.NameMaxLength, MinimumLength = 1)]
    string ContactName,
    [param: Required]
    [param: EmailAddress]
    [param: StringLength(ConsultationRequest.EmailMaxLength)]
    string Email,
    [param: Required]
    [param: StringLength(ConsultationRequest.PhoneNumberMaxLength)]
    [param: RegularExpression(@"^\+?[0-9\s().-]+$")]
    string PhoneNumber,
    [param: Range(1, int.MaxValue)]
    int? ListingId,
    [param: StringLength(ConsultationRequest.MessageMaxLength)]
    string? Message);
