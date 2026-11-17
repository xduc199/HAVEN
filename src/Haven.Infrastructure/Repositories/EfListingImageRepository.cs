using Haven.Application.Abstractions;
using Haven.Domain.Properties;
using Haven.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Haven.Infrastructure.Repositories;

public sealed class EfListingImageRepository : IListingImageRepository
{
    private readonly ApplicationDbContext _db;
    public EfListingImageRepository(ApplicationDbContext db) => _db = db;

    public Task<Listing?> GetListingAsync(int id, CancellationToken cancellationToken = default) =>
        _db.Listings.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<ListingImage?> GetImageAsync(int id, CancellationToken cancellationToken = default) =>
        _db.ListingImages.Include(x => x.Listing).FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<IReadOnlyList<ListingImage>> GetImagesAsync(int listingId, CancellationToken cancellationToken = default) =>
        await _db.ListingImages.Where(x => x.ListingId == listingId).OrderBy(x => x.SortOrder).ThenBy(x => x.Id).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<ListingImage>> GetPrimaryImagesAsync(IReadOnlyCollection<int> listingIds, CancellationToken cancellationToken = default) =>
        listingIds.Count == 0 ? [] : await _db.ListingImages.AsNoTracking()
            .Where(x => listingIds.Contains(x.ListingId) && x.IsPrimary)
            .ToListAsync(cancellationToken);

    public Task AddRangeAsync(IReadOnlyCollection<ListingImage> images, CancellationToken cancellationToken = default) =>
        _db.ListingImages.AddRangeAsync(images, cancellationToken);

    public void Remove(ListingImage image) => _db.ListingImages.Remove(image);
    public void RemoveListing(Listing listing) => _db.Listings.Remove(listing);

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) => _db.SaveChangesAsync(cancellationToken);
}
