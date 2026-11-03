using Haven.Application.Abstractions;
using Haven.Application.Consultations;
using Haven.Application.Listings;
using Haven.Domain.Consultations;
using Haven.Domain.Properties;

namespace Haven.Application.Dashboard;

public sealed class AdminDashboardApplicationService
{
    private readonly IListingRepository _listingRepository;
    private readonly IConsultationRepository _consultationRepository;
    private readonly IAdminAnalyticsRepository _analyticsRepository;
    private readonly TimeProvider _timeProvider;

    public AdminDashboardApplicationService(
        IListingRepository listingRepository,
        IConsultationRepository consultationRepository,
        IAdminAnalyticsRepository analyticsRepository,
        TimeProvider timeProvider)
    {
        _listingRepository = listingRepository;
        _consultationRepository = consultationRepository;
        _analyticsRepository = analyticsRepository;
        _timeProvider = timeProvider;
    }

    public async Task<AdminDashboardSummary> GetSummaryAsync(
        CancellationToken cancellationToken = default)
    {
        var listingCounts = await _listingRepository.GetStatusCountsAsync(
            cancellationToken);
        var consultationCounts =
            await _consultationRepository.GetStatusCountsAsync(cancellationToken);

        return new AdminDashboardSummary(
            listingCounts.Sum(count => count.Count),
            GetCount(listingCounts, ListingStatus.Pending),
            GetCount(listingCounts, ListingStatus.Approved),
            GetCount(consultationCounts, ConsultationStatus.New),
            GetCount(consultationCounts, ConsultationStatus.InProgress));
    }

    public async Task<AdminAnalyticsReport> GetAnalyticsAsync(
        DateOnly? fromDate = null,
        DateOnly? toDate = null,
        CancellationToken cancellationToken = default)
    {
        var today = DateOnly.FromDateTime(_timeProvider.GetUtcNow().UtcDateTime);
        var range = ValidateRange(
            fromDate ?? today.AddDays(-29),
            toDate ?? today);
        return await BuildReportAsync(range, cancellationToken);
    }

    public async Task<AdminAnalyticsReport> GetDashboardAsync(
        DateOnly? fromDate = null,
        DateOnly? toDate = null,
        CancellationToken cancellationToken = default)
    {
        var today = DateOnly.FromDateTime(_timeProvider.GetUtcNow().UtcDateTime);
        var range = ValidateRange(fromDate ?? today.AddDays(-6), toDate ?? today);
        return await BuildReportAsync(range, cancellationToken);
    }

    private async Task<AdminAnalyticsReport> BuildReportAsync(
        AnalyticsDateRange range,
        CancellationToken cancellationToken)
    {
        var snapshot = await _analyticsRepository.GetSnapshotAsync(
            range.FromDate,
            range.ToDate,
            cancellationToken);
        var listingCounts = snapshot.ListingStatusCounts;
        var consultationCounts = snapshot.ConsultationStatusCounts;
        return new AdminAnalyticsReport(
            range,
            new AdminDashboardSummary(
                listingCounts.Sum(count => count.Count),
                GetCount(listingCounts, ListingStatus.Pending),
                GetCount(listingCounts, ListingStatus.Approved),
                GetCount(consultationCounts, ConsultationStatus.New),
                GetCount(consultationCounts, ConsultationStatus.InProgress)),
            GetCount(listingCounts, ListingStatus.Draft),
            GetCount(listingCounts, ListingStatus.Rejected),
            GetCount(listingCounts, ListingStatus.Hidden),
            consultationCounts.Sum(count => count.Count),
            GetCount(consultationCounts, ConsultationStatus.Completed),
            GetCount(consultationCounts, ConsultationStatus.Cancelled),
            snapshot.TotalUsers,
            FillDailyRange(snapshot.ListingsByDay, range),
            FillDailyRange(snapshot.ConsultationsByDay, range),
            EnumCounts(listingCounts),
            snapshot.ListingsByPropertyType,
            EnumCounts(consultationCounts),
            snapshot.LatestListings,
            snapshot.PendingListings,
            snapshot.LatestConsultations);
    }

    public static AnalyticsDateRange ValidateRange(DateOnly fromDate, DateOnly toDate)
    {
        if (toDate < fromDate)
        {
            throw new AnalyticsRangeException("The end date must be on or after the start date.");
        }

        if (toDate.DayNumber - fromDate.DayNumber > 365)
        {
            throw new AnalyticsRangeException("The selected date range cannot exceed 366 days.");
        }

        if (toDate == DateOnly.MaxValue)
        {
            throw new AnalyticsRangeException("The end date is outside the supported range.");
        }

        return new AnalyticsDateRange(fromDate, toDate);
    }

    private static IReadOnlyList<DailyAnalyticsCount> FillDailyRange(
        IReadOnlyList<DailyAnalyticsCount> values,
        AnalyticsDateRange range)
    {
        var counts = values.ToDictionary(value => value.Date, value => value.Count);
        var result = new List<DailyAnalyticsCount>(range.ToDate.DayNumber - range.FromDate.DayNumber + 1);
        for (var date = range.FromDate; date <= range.ToDate; date = date.AddDays(1))
        {
            result.Add(new DailyAnalyticsCount(date, counts.GetValueOrDefault(date)));
        }

        return result;
    }

    private static IReadOnlyList<LabeledAnalyticsCount> EnumCounts(
        IReadOnlyList<ListingStatusCount> counts) =>
        Enum.GetValues<ListingStatus>()
            .Select(status => new LabeledAnalyticsCount(
                status.ToString(),
                GetCount(counts, status)))
            .ToArray();

    private static IReadOnlyList<LabeledAnalyticsCount> EnumCounts(
        IReadOnlyList<ConsultationStatusCount> counts) =>
        Enum.GetValues<ConsultationStatus>()
            .Select(status => new LabeledAnalyticsCount(
                status.ToString(),
                GetCount(counts, status)))
            .ToArray();

    private static long GetCount(
        IReadOnlyList<ListingStatusCount> counts,
        ListingStatus status) =>
        counts.FirstOrDefault(count => count.Status == status)?.Count ?? 0;

    private static long GetCount(
        IReadOnlyList<ConsultationStatusCount> counts,
        ConsultationStatus status) =>
        counts.FirstOrDefault(count => count.Status == status)?.Count ?? 0;
}
