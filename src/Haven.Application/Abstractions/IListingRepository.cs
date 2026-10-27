using Haven.Domain.Properties;

namespace Haven.Application.Abstractions;

public interface IListingRepository
{
    Task<Listing?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<Listing?> GetPublicByIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        Listing listing,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(
        CancellationToken cancellationToken = default);
}
