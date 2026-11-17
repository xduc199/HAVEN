using Haven.Application.Abstractions;
using Haven.Domain.Properties;

namespace Haven.Application.Listings;

public sealed record ListingImageInfo(int Id, int ListingId, string ContentType, long Length, int SortOrder, bool IsPrimary);
public sealed record ListingImageContent(Stream Content, string ContentType);
public sealed record ListingImageUpload(Stream Content, long Length, string FileName, string ContentType);

public sealed class ListingImageApplicationService
{
    public const int MaximumImagesPerListing = 10;
    public const long MaximumFileLength = 10 * 1024 * 1024;
    private readonly IListingImageRepository _repository;
    private readonly IListingImageStorage _storage;
    private readonly TimeProvider _timeProvider;

    public ListingImageApplicationService(IListingImageRepository repository, IListingImageStorage storage, TimeProvider timeProvider)
    {
        _repository = repository;
        _storage = storage;
        _timeProvider = timeProvider;
    }

    public async Task<IReadOnlyList<ListingImageInfo>?> GetOwnedImagesAsync(string ownerId, int listingId, CancellationToken cancellationToken = default)
    {
        var listing = await _repository.GetListingAsync(listingId, cancellationToken);
        if (listing is null || !IsOwner(listing, ownerId)) return null;
        return (await _repository.GetImagesAsync(listingId, cancellationToken)).Select(ToInfo).ToArray();
    }

    public async Task<IReadOnlyList<ListingImageInfo>> GetPublicImagesAsync(int listingId, CancellationToken cancellationToken = default)
    {
        var listing = await _repository.GetListingAsync(listingId, cancellationToken);
        if (listing?.Status != ListingStatus.Approved) return [];
        return (await _repository.GetImagesAsync(listingId, cancellationToken)).Select(ToInfo).ToArray();
    }

    public async Task<IReadOnlyList<ListingImageInfo>> GetImagesForAdminAsync(int listingId, CancellationToken cancellationToken = default)
    {
        var listing = await _repository.GetListingAsync(listingId, cancellationToken);
        if (listing is null) return [];
        return (await _repository.GetImagesAsync(listingId, cancellationToken)).Select(ToInfo).ToArray();
    }

    public async Task<IReadOnlyDictionary<int, int>> GetPrimaryImageIdsAsync(IReadOnlyCollection<int> listingIds, CancellationToken cancellationToken = default)
    {
        var images = await _repository.GetPrimaryImagesAsync(listingIds, cancellationToken);
        return images.ToDictionary(image => image.ListingId, image => image.Id);
    }

    public async Task<ListingImageContent?> OpenAsync(int imageId, string? viewerId, bool isAdmin, CancellationToken cancellationToken = default)
    {
        var image = await _repository.GetImageAsync(imageId, cancellationToken);
        if (image is null || (image.Listing.Status != ListingStatus.Approved && !isAdmin && !IsOwner(image.Listing, viewerId))) return null;
        return new ListingImageContent(await _storage.OpenReadAsync(image.StorageKey, cancellationToken), image.ContentType);
    }

    public async Task<IReadOnlyList<ListingImageInfo>> UploadAsync(string ownerId, int listingId, IReadOnlyCollection<ListingImageUpload> uploads, bool isAdmin = false, CancellationToken cancellationToken = default)
    {
        if (uploads.Count == 0) return [];
        var prepared = new List<(MemoryStream Stream, string Extension, string ContentType, long Length)>();
        var storedKeys = new List<string>();
        try
        {
            if (uploads.Count > MaximumImagesPerListing) throw new ArgumentException($"Tối đa {MaximumImagesPerListing} ảnh mỗi lần tải lên.");
            var listing = await _repository.GetListingAsync(listingId, cancellationToken);
            EnsureCanManage(listing, ownerId, isAdmin);
            var existing = await _repository.GetImagesAsync(listingId, cancellationToken);
            if (existing.Count + uploads.Count > MaximumImagesPerListing) throw new ArgumentException($"Mỗi tin đăng chỉ được có tối đa {MaximumImagesPerListing} ảnh.");

            foreach (var upload in uploads)
            {
                if (upload.Length is <= 0 or > MaximumFileLength) throw new ArgumentException("Mỗi ảnh phải có dung lượng từ 1 byte đến 10 MB.");
                var ext = Path.GetExtension(upload.FileName).ToLowerInvariant();
                var expected = ext switch { ".jpg" or ".jpeg" => ("image/jpeg", ".jpg"), ".png" => ("image/png", ".png"), ".webp" => ("image/webp", ".webp"), _ => ("", "") };
                if (expected.Item1.Length == 0 || !string.Equals(upload.ContentType, expected.Item1, StringComparison.OrdinalIgnoreCase)) throw new ArgumentException("Chỉ chấp nhận ảnh JPEG, PNG hoặc WebP có phần mở rộng và MIME type khớp.");
                var buffer = new MemoryStream();
                try
                {
                    var chunk = new byte[81920];
                    int read;
                    while ((read = await upload.Content.ReadAsync(chunk, cancellationToken)) > 0)
                    {
                        if (buffer.Length + read > MaximumFileLength) throw new ArgumentException("Ảnh vượt quá dung lượng tối đa 10 MB.");
                        await buffer.WriteAsync(chunk.AsMemory(0, read), cancellationToken);
                    }
                }
                catch { buffer.Dispose(); throw; }
                if (buffer.Length != upload.Length || !HasValidSignature(buffer.GetBuffer().AsSpan(0, (int)buffer.Length), expected.Item1))
                {
                    buffer.Dispose();
                    throw new ArgumentException("Nội dung tệp không phải ảnh hợp lệ hoặc dung lượng không khớp.");
                }
                buffer.Position = 0;
                prepared.Add((buffer, expected.Item2, expected.Item1, buffer.Length));
            }
            var newImages = new List<ListingImage>();
            foreach (var item in prepared)
            {
                var key = await _storage.SaveAsync(item.Stream, item.Extension, cancellationToken);
                storedKeys.Add(key);
                newImages.Add(ListingImage.Create(listingId, key, item.ContentType, item.Length, existing.Count + newImages.Count, existing.Count == 0 && newImages.Count == 0, _timeProvider.GetUtcNow()));
            }
            if (!isAdmin) MarkApprovedListingForReview(listing!);
            await _repository.AddRangeAsync(newImages, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);
            return newImages.Select(ToInfo).ToArray();
        }
        catch
        {
            foreach (var key in storedKeys)
            {
                try { await _storage.DeleteAsync(key, CancellationToken.None); } catch { /* retain original failure */ }
            }
            throw;
        }
        finally
        {
            foreach (var item in prepared) item.Stream.Dispose();
            foreach (var upload in uploads) await upload.Content.DisposeAsync();
        }
    }

    public async Task<bool> DeleteAsync(string ownerId, int listingId, int imageId, bool isAdmin = false, CancellationToken cancellationToken = default)
    {
        var image = await _repository.GetImageAsync(imageId, cancellationToken);
        if (image is null || image.ListingId != listingId) return false;
        EnsureCanManage(image.Listing, ownerId, isAdmin);
        var remaining = (await _repository.GetImagesAsync(listingId, cancellationToken)).Where(x => x.Id != imageId).ToArray();
        _repository.Remove(image);
        if (image.IsPrimary && remaining.Length > 0) remaining[0].SetPrimary(true);
        for (var i = 0; i < remaining.Length; i++) remaining[i].SetSortOrder(i);
        if (!isAdmin) MarkApprovedListingForReview(image.Listing);
        await _repository.SaveChangesAsync(cancellationToken);
        await _storage.DeleteAsync(image.StorageKey, cancellationToken);
        return true;
    }

    public async Task<bool> SetPrimaryAsync(string ownerId, int listingId, int imageId, bool isAdmin = false, CancellationToken cancellationToken = default)
    {
        var image = await _repository.GetImageAsync(imageId, cancellationToken);
        if (image is null || image.ListingId != listingId) return false;
        EnsureCanManage(image.Listing, ownerId, isAdmin);
        var images = await _repository.GetImagesAsync(listingId, cancellationToken);
        foreach (var candidate in images) candidate.SetPrimary(candidate.Id == imageId);
        if (!isAdmin) MarkApprovedListingForReview(image.Listing);
        await _repository.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteOwnedListingAsync(string ownerId, int listingId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(ownerId);
        var listing = await _repository.GetListingAsync(listingId, cancellationToken);
        if (listing is null || !IsOwner(listing, ownerId)) return false;
        if (!listing.CanBeDeletedByOwner) throw new InvalidOperationException("Only draft or rejected listings can be deleted by their owner.");
        var keys = (await _repository.GetImagesAsync(listingId, cancellationToken)).Select(image => image.StorageKey).ToArray();
        _repository.RemoveListing(listing);
        await _repository.SaveChangesAsync(cancellationToken);
        foreach (var key in keys) await _storage.DeleteAsync(key, cancellationToken);
        return true;
    }

    private static void EnsureCanManage(Listing? listing, string ownerId, bool isAdmin)
    {
        if (listing is null) throw new KeyNotFoundException("Không tìm thấy tin đăng.");
        if (isAdmin) return;
        if (!IsOwner(listing, ownerId)) throw new UnauthorizedAccessException();
        if (listing.Status is not (ListingStatus.Draft or ListingStatus.Rejected or ListingStatus.Approved))
            throw new InvalidOperationException("Tin đang chờ duyệt hoặc đã ẩn nên không thể quản lý ảnh.");
    }

    private static bool IsOwner(Listing listing, string? ownerId) => !string.IsNullOrWhiteSpace(ownerId) && string.Equals(listing.OwnerId, ownerId, StringComparison.Ordinal);

    private void MarkApprovedListingForReview(Listing listing)
    {
        if (listing.Status == ListingStatus.Approved)
            listing.UpdateCoreDetails(listing.PropertyTypeId, listing.Title, listing.Description, listing.Purpose, listing.PriceAmount, listing.AreaSqm, listing.Province, listing.District, listing.AddressLine, _timeProvider.GetUtcNow(), listing.Bedrooms, listing.Bathrooms, listing.ProvinceCode, listing.WardCode);
    }

    private static bool HasValidSignature(ReadOnlySpan<byte> data, string contentType) => contentType switch
    {
        "image/jpeg" => data.Length >= 5 && data[0] == 0xff && data[1] == 0xd8 && data[2] == 0xff && data[^2] == 0xff && data[^1] == 0xd9,
        "image/png" => data.Length >= 20 && data[..8].SequenceEqual(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }) && data[^8..^4].SequenceEqual("IEND"u8),
        "image/webp" => data.Length >= 12 && data[..4].SequenceEqual("RIFF"u8) && BitConverter.ToUInt32(data.Slice(4, 4)) == data.Length - 8 && data.Slice(8, 4).SequenceEqual("WEBP"u8),
        _ => false
    };

    private static ListingImageInfo ToInfo(ListingImage image) => new(image.Id, image.ListingId, image.ContentType, image.Length, image.SortOrder, image.IsPrimary);
}
