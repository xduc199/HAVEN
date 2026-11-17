using Haven.Application.Abstractions;
using Haven.Domain.Properties;
using System.Buffers.Binary;

namespace Haven.Application.Listings;

public sealed class ListingVideoOptions
{
    public int MaximumVideosPerListing { get; set; } = 3;
    public long MaximumFileLengthBytes { get; set; } = 100 * 1024 * 1024;
}

public sealed record ListingVideoInfo(int Id, int ListingId, string ContentType, long Length, int SortOrder);
public sealed record ListingVideoContent(Stream Content, string ContentType, long Length);
public sealed record ListingVideoUpload(Stream Content, long Length, string FileName, string ContentType);

public sealed class ListingVideoApplicationService
{
    private readonly IListingVideoRepository _repository;
    private readonly IListingVideoStorage _storage;
    private readonly ListingVideoOptions _options;
    private readonly TimeProvider _timeProvider;

    public ListingVideoApplicationService(IListingVideoRepository repository, IListingVideoStorage storage, ListingVideoOptions options, TimeProvider timeProvider)
    {
        _repository = repository;
        _storage = storage;
        _options = options;
        _timeProvider = timeProvider;
    }

    public async Task<IReadOnlyList<ListingVideoInfo>?> GetOwnedVideosAsync(string ownerId, int listingId, CancellationToken cancellationToken = default)
    {
        var listing = await _repository.GetListingAsync(listingId, cancellationToken);
        if (listing is null || !IsOwner(listing, ownerId)) return null;
        return (await _repository.GetVideosAsync(listingId, cancellationToken)).Select(ToInfo).ToArray();
    }

    public async Task<IReadOnlyList<ListingVideoInfo>> GetPublicVideosAsync(int listingId, CancellationToken cancellationToken = default)
    {
        var listing = await _repository.GetListingAsync(listingId, cancellationToken);
        if (listing?.Status != ListingStatus.Approved) return [];
        return (await _repository.GetVideosAsync(listingId, cancellationToken)).Select(ToInfo).ToArray();
    }

    public async Task<IReadOnlyList<ListingVideoInfo>> GetVideosForAdminAsync(int listingId, CancellationToken cancellationToken = default)
    {
        if (await _repository.GetListingAsync(listingId, cancellationToken) is null) return [];
        return (await _repository.GetVideosAsync(listingId, cancellationToken)).Select(ToInfo).ToArray();
    }

    public async Task<ListingVideoContent?> OpenAsync(int videoId, string? viewerId, bool isAdmin, CancellationToken cancellationToken = default)
    {
        var video = await _repository.GetVideoAsync(videoId, cancellationToken);
        if (video is null || (video.Listing.Status != ListingStatus.Approved && !isAdmin && !IsOwner(video.Listing, viewerId))) return null;
        return new ListingVideoContent(await _storage.OpenReadAsync(video.StorageKey, cancellationToken), video.ContentType, video.Length);
    }

    public async Task<IReadOnlyList<ListingVideoInfo>> UploadAsync(string ownerId, int listingId, IReadOnlyCollection<ListingVideoUpload> uploads, bool isAdmin = false, CancellationToken cancellationToken = default)
    {
        if (uploads.Count == 0) return [];
        if (_options.MaximumVideosPerListing is < 1 or > 3) throw new InvalidOperationException("Cấu hình số video tối đa phải nằm trong khoảng 1–3.");
        if (_options.MaximumFileLengthBytes is < 1 or > 100L * 1024 * 1024) throw new InvalidOperationException("Cấu hình dung lượng video tối đa phải nằm trong khoảng 1–100 MB.");
        var storedKeys = new List<string>();
        try
        {
            if (uploads.Count > _options.MaximumVideosPerListing) throw new ArgumentException($"Tối đa {_options.MaximumVideosPerListing} video mỗi tin đăng.");
            var listing = await _repository.GetListingAsync(listingId, cancellationToken);
            EnsureCanManage(listing, ownerId, isAdmin);
            var existing = await _repository.GetVideosAsync(listingId, cancellationToken);
            if (existing.Count + uploads.Count > _options.MaximumVideosPerListing) throw new ArgumentException($"Mỗi tin đăng chỉ được có tối đa {_options.MaximumVideosPerListing} video.");

            var videos = new List<ListingVideo>();
            foreach (var upload in uploads)
            {
                if (upload.Length <= 0 || upload.Length > _options.MaximumFileLengthBytes) throw new ArgumentException($"Mỗi video phải có dung lượng từ 1 byte đến {_options.MaximumFileLengthBytes / (1024 * 1024)} MB.");
                var extension = Path.GetExtension(upload.FileName).ToLowerInvariant();
                var (contentType, fileExtension) = extension switch
                {
                    ".mp4" => ("video/mp4", ".mp4"),
                    ".webm" => ("video/webm", ".webm"),
                    _ => (string.Empty, string.Empty)
                };
                if (contentType.Length == 0 || !string.Equals(contentType, upload.ContentType, StringComparison.OrdinalIgnoreCase)) throw new ArgumentException("Chỉ chấp nhận tệp MP4 hoặc WebM có phần mở rộng và MIME type khớp.");
                var header = new byte[12];
                var headerLength = await ReadHeaderAsync(upload.Content, header, cancellationToken);
                if (!HasValidContainerHeader(header.AsSpan(0, headerLength), contentType, upload.Length)) throw new ArgumentException("Tệp không có chữ ký MP4 hoặc WebM hợp lệ.");
                var storedKey = await _storage.SaveAsync(new PrefixReadStream(header.AsMemory(0, headerLength), upload.Content), fileExtension, _options.MaximumFileLengthBytes, upload.Length, cancellationToken);
                storedKeys.Add(storedKey);
                videos.Add(ListingVideo.Create(listingId, storedKey, contentType, upload.Length, existing.Count + videos.Count, _timeProvider.GetUtcNow()));
            }

            if (!isAdmin) MarkApprovedListingForReview(listing!);
            await _repository.AddRangeAsync(videos, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);
            return videos.Select(ToInfo).ToArray();
        }
        catch
        {
            foreach (var key in storedKeys)
            {
                try { await _storage.DeleteAsync(key, CancellationToken.None); } catch { /* preserve original failure */ }
            }
            throw;
        }
        finally
        {
            foreach (var upload in uploads) await upload.Content.DisposeAsync();
        }
    }

    public async Task<bool> DeleteAsync(string ownerId, int listingId, int videoId, bool isAdmin = false, CancellationToken cancellationToken = default)
    {
        var video = await _repository.GetVideoAsync(videoId, cancellationToken);
        if (video is null || video.ListingId != listingId) return false;
        EnsureCanManage(video.Listing, ownerId, isAdmin);
        _repository.Remove(video);
        if (!isAdmin) MarkApprovedListingForReview(video.Listing);
        await _repository.SaveChangesAsync(cancellationToken);
        await _storage.DeleteAsync(video.StorageKey, cancellationToken);
        return true;
    }

    public async Task<bool> DeleteOwnedVideosAsync(string ownerId, int listingId, CancellationToken cancellationToken = default)
    {
        var listing = await _repository.GetListingAsync(listingId, cancellationToken);
        if (listing is null || !IsOwner(listing, ownerId)) return false;
        if (!listing.CanBeDeletedByOwner) throw new InvalidOperationException("Only draft or rejected listings can be deleted by their owner.");
        var videos = await _repository.GetVideosAsync(listingId, cancellationToken);
        foreach (var video in videos) _repository.Remove(video);
        await _repository.SaveChangesAsync(cancellationToken);
        foreach (var video in videos) await _storage.DeleteAsync(video.StorageKey, cancellationToken);
        return true;
    }

    private async Task<int> ReadHeaderAsync(Stream source, Memory<byte> buffer, CancellationToken cancellationToken)
    {
        var read = 0;
        while (read < buffer.Length)
        {
            var count = await source.ReadAsync(buffer[read..], cancellationToken);
            if (count == 0) break;
            read += count;
        }
        return read;
    }

    private static bool HasValidContainerHeader(ReadOnlySpan<byte> header, string contentType, long length) => contentType switch
    {
        "video/mp4" => header.Length >= 12 && header[4..8].SequenceEqual("ftyp"u8) && BinaryPrimitives.ReadUInt32BigEndian(header[..4]) >= 12 && BinaryPrimitives.ReadUInt32BigEndian(header[..4]) <= length,
        "video/webm" => header.Length >= 4 && header[..4].SequenceEqual(new byte[] { 0x1a, 0x45, 0xdf, 0xa3 }),
        _ => false
    };

    private void EnsureCanManage(Listing? listing, string ownerId, bool isAdmin)
    {
        if (listing is null) throw new KeyNotFoundException("Không tìm thấy tin đăng.");
        if (isAdmin) return;
        if (!IsOwner(listing, ownerId)) throw new UnauthorizedAccessException();
        if (listing.Status is not (ListingStatus.Draft or ListingStatus.Rejected or ListingStatus.Approved)) throw new InvalidOperationException("Tin đang chờ duyệt hoặc đã ẩn nên không thể quản lý video.");
    }

    private void MarkApprovedListingForReview(Listing listing)
    {
        if (listing.Status == ListingStatus.Approved)
            listing.UpdateCoreDetails(listing.PropertyTypeId, listing.Title, listing.Description, listing.Purpose, listing.PriceAmount, listing.AreaSqm, listing.Province, listing.District, listing.AddressLine, _timeProvider.GetUtcNow(), listing.Bedrooms, listing.Bathrooms, listing.ProvinceCode, listing.WardCode);
    }

    private static bool IsOwner(Listing listing, string? ownerId) => !string.IsNullOrWhiteSpace(ownerId) && string.Equals(listing.OwnerId, ownerId, StringComparison.Ordinal);
    private static ListingVideoInfo ToInfo(ListingVideo video) => new(video.Id, video.ListingId, video.ContentType, video.Length, video.SortOrder);

    private sealed class PrefixReadStream(ReadOnlyMemory<byte> prefix, Stream source) : Stream
    {
        private int _offset;
        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => throw new NotSupportedException();
        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        public override int Read(byte[] buffer, int offset, int count)
        {
            var copied = Math.Min(count, prefix.Length - _offset);
            if (copied > 0) { prefix.Span.Slice(_offset, copied).CopyTo(buffer.AsSpan(offset, copied)); _offset += copied; return copied; }
            return source.Read(buffer, offset, count);
        }
        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            var copied = Math.Min(buffer.Length, prefix.Length - _offset);
            if (copied > 0) { prefix.Span.Slice(_offset, copied).CopyTo(buffer.Span); _offset += copied; return ValueTask.FromResult(copied); }
            return source.ReadAsync(buffer, cancellationToken);
        }
        protected override void Dispose(bool disposing) { }
        public override ValueTask DisposeAsync() => ValueTask.CompletedTask;
        public override void Flush() => throw new NotSupportedException();
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    }
}
