using Haven.Domain.Properties;

namespace Haven.Application.Tags;

public sealed record TagPage(
    IReadOnlyList<Tag> Items,
    int Page,
    int PageSize,
    long TotalCount,
    long TotalPages);

public sealed record TagSearchCriteria(string? Search, int Page, int PageSize);

public sealed record TagListingPage(
    IReadOnlyList<Listing> Items,
    int Page,
    int PageSize,
    long TotalCount,
    long TotalPages);

public sealed record TagListingCriteria(int TagId, int Page, int PageSize);

public enum TagAssignmentResult
{
    Added,
    AlreadyAssigned,
    ListingNotFound,
    TagNotFound,
    TagInactive
}

public sealed record PublicTagListing(
    int Id,
    string Name,
    string Slug,
    long ApprovedListingCount);
