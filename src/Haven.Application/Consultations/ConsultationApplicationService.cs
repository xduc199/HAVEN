using Haven.Application.Abstractions;
using Haven.Domain.Consultations;

namespace Haven.Application.Consultations;

public sealed class ConsultationApplicationService
{
    private readonly IConsultationRepository _repository;
    private readonly IListingRepository _listingRepository;
    private readonly TimeProvider _timeProvider;

    public ConsultationApplicationService(
        IConsultationRepository repository,
        IListingRepository listingRepository,
        TimeProvider timeProvider)
    {
        _repository = repository;
        _listingRepository = listingRepository;
        _timeProvider = timeProvider;
    }

    public async Task<ConsultationDetails> CreateAsync(
        string requesterId,
        CreateConsultationCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(requesterId);
        ArgumentNullException.ThrowIfNull(command);

        if (command.ListingId is { } listingId &&
            await _listingRepository.GetPublicByIdAsync(
                listingId,
                cancellationToken) is null)
        {
            throw new ConsultationListingNotFoundException(listingId);
        }

        var request = ConsultationRequest.Create(
            requesterId,
            command.ContactName,
            command.Email,
            command.PhoneNumber,
            command.ListingId,
            command.Message,
            _timeProvider.GetUtcNow());

        await _repository.AddAsync(request, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
        return ConsultationDetails.From(request);
    }

    public async Task<ConsultationPageResult> GetForRequesterAsync(
        string requesterId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(requesterId);
        ValidatePage(page, pageSize);

        var result = await _repository.GetForRequesterAsync(
            requesterId,
            page,
            pageSize,
            cancellationToken);
        return ToPageResult(result, page, pageSize);
    }

    public async Task<ConsultationDetails?> GetForRequesterAsync(
        string requesterId,
        int id,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(requesterId);
        ValidateId(id);

        var request = await _repository.GetByIdForRequesterAsync(
            id,
            requesterId,
            cancellationToken);
        return request is null ? null : ConsultationDetails.From(request);
    }

    public async Task<ConsultationPageResult> GetForAdminAsync(
        ConsultationStatus? status,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default) =>
        await SearchForAdminAsync(
            new AdminConsultationCriteria(null, status, null, null, page, pageSize),
            cancellationToken);

    public async Task<ConsultationPageResult> SearchForAdminAsync(
        AdminConsultationCriteria criteria,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(criteria);
        ValidatePage(criteria.Page, criteria.PageSize);
        if (criteria.Status is { } value && !Enum.IsDefined(value))
        {
            throw new ArgumentOutOfRangeException(nameof(criteria.Status));
        }

        if (criteria.FromUtc is { } from &&
            criteria.ToUtcExclusive is { } to &&
            to <= from)
        {
            throw new ArgumentException(
                "The end date must be after the start date.",
                nameof(criteria));
        }

        var result = await _repository.SearchForAdminAsync(
            criteria with
            {
                Search = string.IsNullOrWhiteSpace(criteria.Search)
                    ? null
                    : criteria.Search.Trim()
            },
            cancellationToken);
        return ToPageResult(result, criteria.Page, criteria.PageSize);
    }

    public async Task<ConsultationDetails?> GetForAdminAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        ValidateId(id);
        var request = await _repository.GetByIdAsync(id, cancellationToken);
        return request is null ? null : ConsultationDetails.From(request);
    }

    public async Task<ConsultationDetails?> ChangeStatusAsync(
        int id,
        ConsultationStatus status,
        CancellationToken cancellationToken = default)
    {
        ValidateId(id);
        if (!Enum.IsDefined(status))
        {
            throw new ArgumentOutOfRangeException(nameof(status));
        }

        var request = await _repository.GetByIdAsync(id, cancellationToken);
        if (request is null)
        {
            return null;
        }

        request.ChangeStatus(status, _timeProvider.GetUtcNow());
        await _repository.SaveChangesAsync(cancellationToken);
        return ConsultationDetails.From(request);
    }

    private static ConsultationPageResult ToPageResult(
        ConsultationPage page,
        int currentPage,
        int pageSize)
    {
        var totalPages = page.TotalCount / pageSize +
            (page.TotalCount % pageSize == 0 ? 0 : 1);
        return new ConsultationPageResult(
            page.Requests.Select(ConsultationDetails.From).ToArray(),
            currentPage,
            pageSize,
            page.TotalCount,
            totalPages);
    }

    private static void ValidatePage(int page, int pageSize)
    {
        if (page <= 0 ||
            pageSize is < 1 or > 100 ||
            (long)(page - 1) * pageSize > int.MaxValue)
        {
            throw new ArgumentOutOfRangeException(
                nameof(page),
                "The page or page size is invalid.");
        }
    }

    private static void ValidateId(int id)
    {
        if (id <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(id));
        }
    }
}
