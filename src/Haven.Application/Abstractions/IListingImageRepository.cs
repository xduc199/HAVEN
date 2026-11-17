using Haven.Domain.Properties;

namespace Haven.Application.Abstractions;

public interface IListingImageRepository
{
    Task<Listing?> GetListingAsync(int id, CancellationToken cancellationToken = default);
    Task<ListingImage?> GetImageAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ListingImage>> GetImagesAsync(int listingId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ListingImage>> GetPrimaryImagesAsync(IReadOnlyCollection<int> listingIds, CancellationToken cancellationToken = default);
    Task AddRangeAsync(IReadOnlyCollection<ListingImage> images, CancellationToken cancellationToken = default);
    void Remove(ListingImage image);
    void RemoveListing(Listing listing);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}

public interface IListingImageStorage
{
    Task<string> SaveAsync(Stream content, string extension, CancellationToken cancellationToken = default);
    Task<Stream> OpenReadAsync(string storageKey, CancellationToken cancellationToken = default);
    Task DeleteAsync(string storageKey, CancellationToken cancellationToken = default);
}

public interface IListingVideoStorage
{
    Task<string> SaveAsync(Stream content, string extension, long maximumLength, long expectedLength, CancellationToken cancellationToken = default);
    Task<Stream> OpenReadAsync(string storageKey, CancellationToken cancellationToken = default);
    Task DeleteAsync(string storageKey, CancellationToken cancellationToken = default);
}
