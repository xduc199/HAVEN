namespace Haven.Application.Consultations;

public sealed record ConsultationPageResult(
    IReadOnlyList<ConsultationDetails> Items,
    int Page,
    int PageSize,
    long TotalCount,
    long TotalPages);
