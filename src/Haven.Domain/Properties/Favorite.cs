namespace Haven.Domain.Properties;

public sealed class Favorite
{
    public const int UserIdMaxLength = 450;

    private Favorite()
    {
    }

    private Favorite(string userId, int listingId, DateTimeOffset createdAtUtc)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new ArgumentException(
                "User ID is required.",
                nameof(userId));
        }

        if (listingId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(listingId));
        }

        UserId = userId.Trim();
        ListingId = listingId;
        CreatedAtUtc = createdAtUtc;
    }

    public string UserId { get; private set; } = string.Empty;

    public int ListingId { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public static Favorite Create(
        string userId,
        int listingId,
        DateTimeOffset createdAtUtc) =>
        new(userId, listingId, createdAtUtc);
}
