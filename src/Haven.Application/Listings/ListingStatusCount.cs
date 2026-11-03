using Haven.Domain.Properties;

namespace Haven.Application.Listings;

public sealed record ListingStatusCount(ListingStatus Status, long Count);
