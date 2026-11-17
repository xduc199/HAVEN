namespace Haven.Domain.Properties;

public sealed class ListingImage
{
    private ListingImage() { }

    private ListingImage(
        int listingId,
        string storageKey,
        string contentType,
        long length,
        int sortOrder,
        bool isPrimary,
        DateTimeOffset createdAtUtc)
    {
        if (listingId <= 0) throw new ArgumentOutOfRangeException(nameof(listingId));
        ArgumentException.ThrowIfNullOrWhiteSpace(storageKey);
        if (contentType is not ("image/jpeg" or "image/png" or "image/webp"))
            throw new ArgumentException("Unsupported image content type.", nameof(contentType));
        if (length <= 0) throw new ArgumentOutOfRangeException(nameof(length));
        if (sortOrder < 0) throw new ArgumentOutOfRangeException(nameof(sortOrder));

        ListingId = listingId;
        StorageKey = storageKey;
        ContentType = contentType;
        Length = length;
        SortOrder = sortOrder;
        IsPrimary = isPrimary;
        CreatedAtUtc = createdAtUtc;
    }

    public int Id { get; private set; }
    public int ListingId { get; private set; }
    public string StorageKey { get; private set; } = string.Empty;
    public string ContentType { get; private set; } = string.Empty;
    public long Length { get; private set; }
    public int SortOrder { get; private set; }
    public bool IsPrimary { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public Listing Listing { get; private set; } = null!;

    public static ListingImage Create(
        int listingId, string storageKey, string contentType, long length,
        int sortOrder, bool isPrimary, DateTimeOffset createdAtUtc) =>
        new(listingId, storageKey, contentType, length, sortOrder, isPrimary, createdAtUtc);

    public void SetPrimary(bool value) => IsPrimary = value;
    public void SetSortOrder(int value)
    {
        if (value < 0) throw new ArgumentOutOfRangeException(nameof(value));
        SortOrder = value;
    }
}
