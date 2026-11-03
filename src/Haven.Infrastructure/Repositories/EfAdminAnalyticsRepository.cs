using Haven.Application.Abstractions;
using Haven.Application.Consultations;
using Haven.Application.Dashboard;
using Haven.Application.Listings;
using Haven.Domain.Consultations;
using Haven.Domain.Properties;
using Haven.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Haven.Infrastructure.Repositories;

public sealed class EfAdminAnalyticsRepository(ApplicationDbContext dbContext)
    : IAdminAnalyticsRepository
{
    public async Task<AdminAnalyticsSnapshot> GetSnapshotAsync(
        DateOnly fromDate,
        DateOnly toDate,
        CancellationToken cancellationToken = default)
    {
        var startUtc = new DateTimeOffset(
            fromDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc));
        var endUtc = new DateTimeOffset(
            toDate.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc));

        var listingQuery = dbContext.Listings.AsNoTracking().Where(listing =>
            listing.CreatedAtUtc >= startUtc && listing.CreatedAtUtc < endUtc);
        var consultationQuery = dbContext.ConsultationRequests.AsNoTracking().Where(request =>
            request.CreatedAtUtc >= startUtc && request.CreatedAtUtc < endUtc);

        var listingStatusCounts = await listingQuery
            .GroupBy(listing => listing.Status)
            .Select(group => new ListingStatusCount(group.Key, group.LongCount()))
            .ToListAsync(cancellationToken);
        var consultationStatusCounts = await consultationQuery
            .GroupBy(request => request.Status)
            .Select(group => new ConsultationStatusCount(group.Key, group.LongCount()))
            .ToListAsync(cancellationToken);

        var listingGroups = await listingQuery
            .GroupBy(listing => new
            {
                listing.CreatedAtUtc.Year,
                listing.CreatedAtUtc.Month,
                listing.CreatedAtUtc.Day
            })
            .Select(group => new
            {
                group.Key.Year,
                group.Key.Month,
                group.Key.Day,
                Count = group.LongCount()
            })
            .ToListAsync(cancellationToken);
        var consultationGroups = await consultationQuery
            .GroupBy(request => new
            {
                request.CreatedAtUtc.Year,
                request.CreatedAtUtc.Month,
                request.CreatedAtUtc.Day
            })
            .Select(group => new
            {
                group.Key.Year,
                group.Key.Month,
                group.Key.Day,
                Count = group.LongCount()
            })
            .ToListAsync(cancellationToken);

        var propertyTypeCounts = await (
                from listing in listingQuery
                join propertyType in dbContext.PropertyTypes.AsNoTracking()
                    on listing.PropertyTypeId equals propertyType.Id
                group listing by propertyType.Name
                into grouping
                orderby grouping.LongCount() descending, grouping.Key
                select new LabeledAnalyticsCount(grouping.Key, grouping.LongCount()))
            .ToListAsync(cancellationToken);

        var latestListings = await (
                from listing in listingQuery
                join propertyType in dbContext.PropertyTypes.AsNoTracking()
                    on listing.PropertyTypeId equals propertyType.Id
                orderby listing.CreatedAtUtc descending, listing.Id
                select new AdminLatestListing(
                    listing.Id,
                    listing.Title,
                    listing.Status,
                    propertyType.Name,
                    listing.CreatedAtUtc))
            .Take(8)
            .ToListAsync(cancellationToken);

        var pendingListings = await (
                from listing in dbContext.Listings.AsNoTracking()
                where listing.Status == ListingStatus.Pending
                join propertyType in dbContext.PropertyTypes.AsNoTracking()
                    on listing.PropertyTypeId equals propertyType.Id
                orderby listing.SubmittedAtUtc descending, listing.Id
                select new AdminLatestListing(
                    listing.Id,
                    listing.Title,
                    listing.Status,
                    propertyType.Name,
                    listing.CreatedAtUtc))
            .Take(8)
            .ToListAsync(cancellationToken);

        var latestConsultations = await consultationQuery
            .OrderByDescending(request => request.CreatedAtUtc)
            .ThenBy(request => request.Id)
            .Select(request => new AdminLatestConsultation(
                request.Id,
                request.ContactName,
                request.Email,
                request.PhoneNumber,
                request.Status,
                request.CreatedAtUtc))
            .Take(8)
            .ToListAsync(cancellationToken);

        return new AdminAnalyticsSnapshot(
            listingStatusCounts,
            consultationStatusCounts,
            listingGroups.Select(group => new DailyAnalyticsCount(
                new DateOnly(group.Year, group.Month, group.Day),
                group.Count)).ToArray(),
            consultationGroups.Select(group => new DailyAnalyticsCount(
                new DateOnly(group.Year, group.Month, group.Day),
                group.Count)).ToArray(),
            propertyTypeCounts,
            latestListings,
            pendingListings,
            latestConsultations,
            await dbContext.Users.LongCountAsync(cancellationToken));
    }
}
