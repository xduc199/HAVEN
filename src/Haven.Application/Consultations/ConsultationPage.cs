using Haven.Domain.Consultations;

namespace Haven.Application.Consultations;

public sealed record ConsultationPage(
    IReadOnlyList<ConsultationRequest> Requests,
    long TotalCount);
