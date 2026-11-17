using Haven.Application.Abstractions;
using Haven.Domain.Properties;
using Haven.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Haven.Infrastructure.Repositories;

public sealed class EfListingVideoRepository(ApplicationDbContext db) : IListingVideoRepository
{
    public Task<Listing?> GetListingAsync(int id, CancellationToken cancellationToken = default) =>
        db.Listings.FirstOrDefaultAsync(listing => listing.Id == id, cancellationToken);

    public Task<ListingVideo?> GetVideoAsync(int id, CancellationToken cancellationToken = default) =>
        db.ListingVideos.Include(video => video.Listing).FirstOrDefaultAsync(video => video.Id == id, cancellationToken);

    public async Task<IReadOnlyList<ListingVideo>> GetVideosAsync(int listingId, CancellationToken cancellationToken = default) =>
        await db.ListingVideos.Where(video => video.ListingId == listingId).OrderBy(video => video.SortOrder).ThenBy(video => video.Id).ToListAsync(cancellationToken);

    public Task AddRangeAsync(IReadOnlyCollection<ListingVideo> videos, CancellationToken cancellationToken = default) =>
        db.ListingVideos.AddRangeAsync(videos, cancellationToken);

    public void Remove(ListingVideo video) => db.ListingVideos.Remove(video);
    public Task SaveChangesAsync(CancellationToken cancellationToken = default) => db.SaveChangesAsync(cancellationToken);
}
