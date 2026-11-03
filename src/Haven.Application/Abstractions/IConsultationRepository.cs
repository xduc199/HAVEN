using Haven.Application.Consultations;
using Haven.Domain.Consultations;

namespace Haven.Application.Abstractions;

public interface IConsultationRepository
{
    Task<ConsultationRequest?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<ConsultationRequest?> GetByIdForRequesterAsync(
        int id,
        string requesterId,
        CancellationToken cancellationToken = default);

    Task<ConsultationPage> GetForRequesterAsync(
        string requesterId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<ConsultationPage> GetForAdminAsync(
        ConsultationStatus? status,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<ConsultationPage> SearchForAdminAsync(
        AdminConsultationCriteria criteria,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ConsultationStatusCount>> GetStatusCountsAsync(
        CancellationToken cancellationToken = default);

    Task AddAsync(
        ConsultationRequest request,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(
        CancellationToken cancellationToken = default);
}
