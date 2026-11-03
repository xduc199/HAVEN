using Haven.Application.Abstractions;
using Haven.Domain.Properties;

namespace Haven.Application.PropertyTypes;

public sealed class PropertyTypeApplicationService
{
    private readonly IPropertyTypeRepository _repository;

    public PropertyTypeApplicationService(IPropertyTypeRepository repository)
    {
        _repository = repository;
    }

    public Task<IReadOnlyList<PropertyType>> GetActiveAsync(
        CancellationToken cancellationToken = default) =>
        _repository.GetActiveAsync(cancellationToken);

    public Task<IReadOnlyList<PropertyType>> GetAllAsync(
        CancellationToken cancellationToken = default) =>
        _repository.GetAllAsync(cancellationToken);

    public async Task<PropertyType> CreateAsync(
        string name,
        int sortOrder,
        bool isActive,
        CancellationToken cancellationToken = default)
    {
        await EnsureUniqueNameAsync(name, excludingId: null, cancellationToken);

        var propertyType = PropertyType.Create(name, sortOrder);
        if (!isActive)
        {
            propertyType.Deactivate();
        }

        await _repository.AddAsync(propertyType, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
        return propertyType;
    }

    public async Task<PropertyType?> UpdateAsync(
        int id,
        string name,
        int sortOrder,
        bool isActive,
        CancellationToken cancellationToken = default)
    {
        if (id <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(id));
        }

        var propertyType = await _repository.GetByIdAsync(id, cancellationToken);
        if (propertyType is null)
        {
            return null;
        }

        await EnsureUniqueNameAsync(name, id, cancellationToken);
        propertyType.Rename(name);
        propertyType.ChangeSortOrder(sortOrder);
        if (isActive)
        {
            propertyType.Activate();
        }
        else
        {
            propertyType.Deactivate();
        }

        await _repository.SaveChangesAsync(cancellationToken);
        return propertyType;
    }

    private async Task EnsureUniqueNameAsync(
        string name,
        int? excludingId,
        CancellationToken cancellationToken)
    {
        var normalizedName = name?.Trim();
        if (string.IsNullOrWhiteSpace(normalizedName))
        {
            throw new ArgumentException(
                "Property type name is required.",
                nameof(name));
        }

        var propertyTypes = await _repository.GetAllAsync(cancellationToken);
        if (propertyTypes.Any(propertyType =>
            propertyType.Id != excludingId &&
            string.Equals(
                propertyType.Name,
                normalizedName,
                StringComparison.OrdinalIgnoreCase)))
        {
            throw new DuplicatePropertyTypeNameException(normalizedName);
        }
    }
}
