namespace Haven.Domain.Properties;

public sealed class ListingTag
{
    private ListingTag() { }

    private ListingTag(int listingId, int tagId)
    {
        ListingId = listingId;
        TagId = tagId;
    }

    public int ListingId { get; private set; }
    public int TagId { get; private set; }
    public Listing Listing { get; private set; } = null!;
    public Tag Tag { get; private set; } = null!;

    public static ListingTag Create(int listingId, int tagId)
    {
        if (listingId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(listingId));
        }

        if (tagId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(tagId));
        }

        return new ListingTag(listingId, tagId);
    }
}
