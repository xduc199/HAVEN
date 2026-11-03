using Haven.Domain.Consultations;

namespace Haven.Application.Consultations;

public sealed record AdminConsultationCriteria(
    string? Search,
    ConsultationStatus? Status,
    DateTimeOffset? FromUtc,
    DateTimeOffset? ToUtcExclusive,
    int Page,
    int PageSize);
