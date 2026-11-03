using System.ComponentModel.DataAnnotations;
using Haven.Domain.Consultations;

namespace Haven.Web.Models.Consultations;

public sealed record ConsultationListRequest(
    [param: EnumDataType(typeof(ConsultationStatus))]
    ConsultationStatus? Status = null,
    [param: Range(1, int.MaxValue)]
    int Page = 1,
    [param: Range(1, 100)]
    int PageSize = 20,
    [param: StringLength(200)]
    string? Search = null,
    DateOnly? FromDate = null,
    DateOnly? ToDate = null);
