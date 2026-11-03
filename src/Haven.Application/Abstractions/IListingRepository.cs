using Haven.Domain.Properties;
using Haven.Application.Listings;
using Haven.Application.Tags;

namespace Haven.Application.Abstractions;

public interface IListingRepository
{
    Task<Listing?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<Listing?> GetPublicByIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<string?> GetOwnerDisplayNameAsync(
        string ownerId,
        CancellationToken cancellationToken = default);

    Task<ListingSearchPage> SearchApprovedAsync(
        ListingSearchCriteria criteria,
        CancellationToken cancellationToken = default);

    Task<ListingSearchPage> GetForOwnerAsync(
        string ownerId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Listing>> GetApprovedByIdsAsync(
        IReadOnlyCollection<int> listingIds,
        CancellationToken cancellationToken = default);

    Task<TagListingPage> GetApprovedByTagAsync(
        int tagId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<ListingSearchPage> GetForAdminAsync(
        ListingStatus? status,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<ListingSearchPage> SearchForAdminAsync(
        AdminListingCriteria criteria,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ListingStatusCount>> GetStatusCountsAsync(
        CancellationToken cancellationToken = default);

    Task AddAsync(
        Listing listing,
        CancellationToken cancellationToken = default);

    Task RemoveAsync(
        Listing listing,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(
        CancellationToken cancellationToken = default);
}
