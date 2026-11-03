namespace Haven.Application.Listings;

public sealed record OwnedListingPage(
    IReadOnlyList<OwnedListingDetails> Items,
    int Page,
    int PageSize,
    long TotalCount,
    long TotalPages);
