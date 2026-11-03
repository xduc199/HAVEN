using System.ComponentModel.DataAnnotations;
using Haven.Domain.Consultations;

namespace Haven.Web.Models.Consultations;

public sealed record UpdateConsultationStatusRequest(
    [param: EnumDataType(typeof(ConsultationStatus))]
    ConsultationStatus Status);
