using Haven.Domain.Properties;

namespace Haven.Application.Listings;

public sealed record ModerationListingDetails(
    int Id,
    string OwnerId,
    int PropertyTypeId,
    string Title,
    string Description,
    ListingPurpose Purpose,
    decimal PriceAmount,
    decimal AreaSqm,
    string Province,
    string District,
    string AddressLine,
    int? Bedrooms,
    int? Bathrooms,
    ListingStatus Status,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc,
    DateTimeOffset? SubmittedAtUtc,
    DateTimeOffset? ApprovedAtUtc,
    string? RejectionReason)
{
    public static ModerationListingDetails From(Listing listing)
    {
        ArgumentNullException.ThrowIfNull(listing);

        return new ModerationListingDetails(
            listing.Id,
            listing.OwnerId,
            listing.PropertyTypeId,
            listing.Title,
            listing.Description,
            listing.Purpose,
            listing.PriceAmount,
            listing.AreaSqm,
            listing.Province,
            listing.District,
            listing.AddressLine,
            listing.Bedrooms,
            listing.Bathrooms,
            listing.Status,
            listing.CreatedAtUtc,
            listing.UpdatedAtUtc,
            listing.SubmittedAtUtc,
            listing.ApprovedAtUtc,
            listing.RejectionReason);
    }
}
