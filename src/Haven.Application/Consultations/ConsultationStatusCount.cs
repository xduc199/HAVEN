using Haven.Domain.Consultations;

namespace Haven.Application.Consultations;

public sealed record ConsultationStatusCount(
    ConsultationStatus Status,
    long Count);
