using Haven.Application.Abstractions;
using Haven.Application.Consultations;
using Haven.Application.Dashboard;
using Haven.Application.Listings;
using Haven.Domain.Consultations;
using Haven.Domain.Properties;
using Moq;

namespace Haven.UnitTests.Dashboard;

public sealed class AdminDashboardApplicationServiceTests
{
    private static readonly DateTimeOffset Now =
        new(2026, 10, 26, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task GetDashboardAsync_AggregatesStatusesAndFillsDaysWithoutActivity()
    {
        var analytics = new Mock<IAdminAnalyticsRepository>();
        analytics.Setup(repository => repository.GetSnapshotAsync(
                new DateOnly(2026, 10, 20),
                new DateOnly(2026, 10, 26),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AdminAnalyticsSnapshot(
                [
                    new ListingStatusCount(ListingStatus.Pending, 2),
                    new ListingStatusCount(ListingStatus.Approved, 3)
                ],
                [
                    new ConsultationStatusCount(ConsultationStatus.New, 1),
                    new ConsultationStatusCount(ConsultationStatus.Completed, 2)
                ],
                [new DailyAnalyticsCount(new DateOnly(2026, 10, 22), 4)],
                [],
                [new LabeledAnalyticsCount("House", 5)],
                [],
                [],
                [],
                7));
        var service = new AdminDashboardApplicationService(
            new Mock<IListingRepository>().Object,
            new Mock<IConsultationRepository>().Object,
            analytics.Object,
            new FixedTimeProvider(Now));

        var result = await service.GetDashboardAsync(
            new DateOnly(2026, 10, 20),
            new DateOnly(2026, 10, 26));

        Assert.Equal(5, result.Kpis.TotalListings);
        Assert.Equal(2, result.Kpis.PendingListings);
        Assert.Equal(1, result.Kpis.NewConsultations);
        Assert.Equal(7, result.TotalUsers);
        Assert.Equal(7, result.ListingsByDay.Count);
        Assert.Equal(4, result.ListingsByDay[2].Count);
        Assert.All(result.ConsultationsByDay, item => Assert.Equal(0, item.Count));
    }

    [Fact]
    public void ValidateRange_RejectsRangesLongerThan366Days()
    {
        Assert.Throws<AnalyticsRangeException>(() =>
            AdminDashboardApplicationService.ValidateRange(
                new DateOnly(2025, 1, 1),
                new DateOnly(2026, 1, 3)));
    }

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}
