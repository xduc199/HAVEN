using Haven.Application.Dashboard;

namespace Haven.Application.Abstractions;

public interface IAdminAnalyticsRepository
{
    Task<AdminAnalyticsSnapshot> GetSnapshotAsync(
        DateOnly fromDate,
        DateOnly toDate,
        CancellationToken cancellationToken = default);
}
