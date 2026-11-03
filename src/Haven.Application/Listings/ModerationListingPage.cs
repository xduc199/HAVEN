namespace Haven.Application.Listings;

public sealed record ModerationListingPage(
    IReadOnlyList<ModerationListingDetails> Items,
    int Page,
    int PageSize,
    long TotalCount,
    long TotalPages);
