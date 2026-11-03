using Haven.Application.Abstractions;
using Haven.Application.Consultations;
using Haven.Domain.Consultations;
using Haven.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Haven.Infrastructure.Repositories;

public sealed class EfConsultationRepository : IConsultationRepository
{
    private readonly ApplicationDbContext _dbContext;

    public EfConsultationRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<ConsultationRequest?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.ConsultationRequests
            .FirstOrDefaultAsync(
                request => request.Id == id,
                cancellationToken);
    }

    public Task<ConsultationRequest?> GetByIdForRequesterAsync(
        int id,
        string requesterId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.ConsultationRequests
            .AsNoTracking()
            .FirstOrDefaultAsync(
                request => request.Id == id &&
                    request.RequesterId == requesterId,
                cancellationToken);
    }

    public async Task<ConsultationPage> GetForRequesterAsync(
        string requesterId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.ConsultationRequests
            .AsNoTracking()
            .Where(request => request.RequesterId == requesterId);
        return await ToPageAsync(query, page, pageSize, cancellationToken);
    }

    public async Task<ConsultationPage> GetForAdminAsync(
        ConsultationStatus? status,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default) =>
        await SearchForAdminAsync(
            new AdminConsultationCriteria(null, status, null, null, page, pageSize),
            cancellationToken);

    public async Task<ConsultationPage> SearchForAdminAsync(
        AdminConsultationCriteria criteria,
        CancellationToken cancellationToken = default)
    {
        IQueryable<ConsultationRequest> query =
            _dbContext.ConsultationRequests.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(criteria.Search))
        {
            var search = criteria.Search.Trim();
            query = query.Where(request =>
                request.ContactName.Contains(search) ||
                request.Email.Contains(search) ||
                request.PhoneNumber.Contains(search));
        }

        if (criteria.Status is { } consultationStatus)
        {
            query = query.Where(
                request => request.Status == consultationStatus);
        }

        if (criteria.FromUtc is { } fromUtc)
        {
            query = query.Where(request => request.CreatedAtUtc >= fromUtc);
        }

        if (criteria.ToUtcExclusive is { } toUtcExclusive)
        {
            query = query.Where(request => request.CreatedAtUtc < toUtcExclusive);
        }

        return await ToPageAsync(
            query,
            criteria.Page,
            criteria.PageSize,
            cancellationToken);
    }

    public async Task<IReadOnlyList<ConsultationStatusCount>> GetStatusCountsAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.ConsultationRequests
            .AsNoTracking()
            .GroupBy(request => request.Status)
            .Select(group => new ConsultationStatusCount(
                group.Key,
                group.LongCount()))
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(
        ConsultationRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        await _dbContext.ConsultationRequests.AddAsync(request, cancellationToken);
    }

    public Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }

    private static async Task<ConsultationPage> ToPageAsync(
        IQueryable<ConsultationRequest> query,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var totalCount = await query.LongCountAsync(cancellationToken);
        var requests = await query
            .OrderByDescending(request => request.CreatedAtUtc)
            .ThenBy(request => request.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
        return new ConsultationPage(requests, totalCount);
    }
}
