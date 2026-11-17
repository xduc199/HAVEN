using Haven.Domain.Properties;

namespace Haven.Application.Abstractions;

public interface IListingVideoRepository
{
    Task<Listing?> GetListingAsync(int id, CancellationToken cancellationToken = default);
    Task<ListingVideo?> GetVideoAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ListingVideo>> GetVideosAsync(int listingId, CancellationToken cancellationToken = default);
    Task AddRangeAsync(IReadOnlyCollection<ListingVideo> videos, CancellationToken cancellationToken = default);
    void Remove(ListingVideo video);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
