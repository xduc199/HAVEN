using Haven.Domain.Properties;

namespace Haven.Application.Abstractions;

public interface IPropertyTypeRepository
{
    Task<PropertyType?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PropertyType>> GetActiveAsync(
        CancellationToken cancellationToken = default);
}