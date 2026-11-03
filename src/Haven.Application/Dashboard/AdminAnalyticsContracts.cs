using Haven.Application.Consultations;
using Haven.Application.Listings;
using Haven.Domain.Consultations;
using Haven.Domain.Properties;

namespace Haven.Application.Dashboard;

public sealed record AnalyticsDateRange(DateOnly FromDate, DateOnly ToDate);

public sealed record DailyAnalyticsCount(DateOnly Date, long Count);

public sealed record LabeledAnalyticsCount(string Label, long Count);

public sealed record AdminLatestListing(
    int Id,
    string Title,
    ListingStatus Status,
    string PropertyTypeName,
    DateTimeOffset CreatedAtUtc);

public sealed record AdminLatestConsultation(
    int Id,
    string ContactName,
    string Email,
    string PhoneNumber,
    ConsultationStatus Status,
    DateTimeOffset CreatedAtUtc);

public sealed record AdminAnalyticsSnapshot(
    IReadOnlyList<ListingStatusCount> ListingStatusCounts,
    IReadOnlyList<ConsultationStatusCount> ConsultationStatusCounts,
    IReadOnlyList<DailyAnalyticsCount> ListingsByDay,
    IReadOnlyList<DailyAnalyticsCount> ConsultationsByDay,
    IReadOnlyList<LabeledAnalyticsCount> ListingsByPropertyType,
    IReadOnlyList<AdminLatestListing> LatestListings,
    IReadOnlyList<AdminLatestListing> PendingListings,
    IReadOnlyList<AdminLatestConsultation> LatestConsultations,
    long TotalUsers);

public sealed record AdminAnalyticsReport(
    AnalyticsDateRange DateRange,
    AdminDashboardSummary Kpis,
    long DraftListings,
    long RejectedListings,
    long HiddenListings,
    long TotalConsultations,
    long CompletedConsultations,
    long CancelledConsultations,
    long TotalUsers,
    IReadOnlyList<DailyAnalyticsCount> ListingsByDay,
    IReadOnlyList<DailyAnalyticsCount> ConsultationsByDay,
    IReadOnlyList<LabeledAnalyticsCount> ListingsByStatus,
    IReadOnlyList<LabeledAnalyticsCount> ListingsByPropertyType,
    IReadOnlyList<LabeledAnalyticsCount> ConsultationsByStatus,
    IReadOnlyList<AdminLatestListing> LatestListings,
    IReadOnlyList<AdminLatestListing> PendingListings,
    IReadOnlyList<AdminLatestConsultation> LatestConsultations);

public sealed class AnalyticsRangeException(string message) : ArgumentException(message)
{
}
