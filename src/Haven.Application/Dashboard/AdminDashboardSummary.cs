namespace Haven.Application.Dashboard;

public sealed record AdminDashboardSummary(
    long TotalListings,
    long PendingListings,
    long ApprovedListings,
    long NewConsultations,
    long InProgressConsultations);
