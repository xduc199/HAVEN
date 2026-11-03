using Haven.Application.Abstractions;
using Haven.Domain.Properties;

namespace Haven.Application.Favorites;

public sealed class FavoriteApplicationService
{
    private readonly IFavoriteRepository _repository;
    private readonly TimeProvider _timeProvider;

    public FavoriteApplicationService(
        IFavoriteRepository repository,
        TimeProvider timeProvider)
    {
        _repository = repository;
        _timeProvider = timeProvider;
    }

    public async Task<AddFavoriteResult> AddAsync(
        string userId,
        int listingId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        ValidateListingId(listingId);

        if (!await _repository.IsApprovedListingAsync(
                listingId,
                cancellationToken))
        {
            return AddFavoriteResult.ListingNotFound;
        }

        if (await _repository.ExistsAsync(
                userId,
                listingId,
                cancellationToken))
        {
            return AddFavoriteResult.AlreadyExists;
        }

        await _repository.AddAsync(
            Favorite.Create(
                userId,
                listingId,
                _timeProvider.GetUtcNow()),
            cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return AddFavoriteResult.Added;
    }

    public Task<bool> IsFavoriteAsync(
        string userId,
        int listingId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        ValidateListingId(listingId);
        return _repository.ExistsAsync(userId, listingId, cancellationToken);
    }

    public async Task<FavoritePage> GetForUserAsync(
        string userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        ValidatePaging(page, pageSize);

        var skip = (page - 1) * pageSize;
        var totalCount = await _repository.CountForUserAsync(
            userId,
            cancellationToken);
        var items = await _repository.GetForUserAsync(
            userId,
            skip,
            pageSize,
            cancellationToken);
        var totalPages = totalCount / pageSize +
            (totalCount % pageSize == 0 ? 0 : 1);

        return new FavoritePage(
            items,
            page,
            pageSize,
            totalCount,
            totalPages);
    }

    public async Task<bool> RemoveAsync(
        string userId,
        int listingId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        ValidateListingId(listingId);

        var removed = await _repository.RemoveAsync(
            userId,
            listingId,
            cancellationToken);
        if (removed)
        {
            await _repository.SaveChangesAsync(cancellationToken);
        }

        return removed;
    }

    private static void ValidateListingId(int listingId)
    {
        if (listingId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(listingId));
        }
    }

    private static void ValidatePaging(int page, int pageSize)
    {
        if (page <= 0 ||
            (long)(page - 1) * pageSize > int.MaxValue)
        {
            throw new ArgumentOutOfRangeException(nameof(page));
        }

        if (pageSize is < 1 or > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(pageSize));
        }
    }
}
