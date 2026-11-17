namespace Haven.Domain.Properties;

public sealed class ListingVideo
{
    private ListingVideo() { }

    private ListingVideo(int listingId, string storageKey, string contentType, long length, int sortOrder, DateTimeOffset createdAtUtc)
    {
        if (listingId <= 0) throw new ArgumentOutOfRangeException(nameof(listingId));
        ArgumentException.ThrowIfNullOrWhiteSpace(storageKey);
        if (contentType is not ("video/mp4" or "video/webm")) throw new ArgumentException("Unsupported video content type.", nameof(contentType));
        if (length <= 0) throw new ArgumentOutOfRangeException(nameof(length));
        if (sortOrder < 0) throw new ArgumentOutOfRangeException(nameof(sortOrder));
        ListingId = listingId;
        StorageKey = storageKey;
        ContentType = contentType;
        Length = length;
        SortOrder = sortOrder;
        CreatedAtUtc = createdAtUtc;
    }

    public int Id { get; private set; }
    public int ListingId { get; private set; }
    public string StorageKey { get; private set; } = string.Empty;
    public string ContentType { get; private set; } = string.Empty;
    public long Length { get; private set; }
    public int SortOrder { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public Listing Listing { get; private set; } = null!;

    public static ListingVideo Create(int listingId, string storageKey, string contentType, long length, int sortOrder, DateTimeOffset createdAtUtc) =>
        new(listingId, storageKey, contentType, length, sortOrder, createdAtUtc);
}
