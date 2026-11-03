using Haven.Application.Abstractions;
using Haven.Domain.Properties;
using Haven.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Haven.Infrastructure.Repositories;

public sealed class EfPropertyTypeRepository : IPropertyTypeRepository
{
    private readonly ApplicationDbContext _dbContext;

    public EfPropertyTypeRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<PropertyType?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.PropertyTypes
            .FirstOrDefaultAsync(
                propertyType => propertyType.Id == id,
                cancellationToken);
    }

    public async Task<IReadOnlyList<PropertyType>> GetActiveAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.PropertyTypes
            .AsNoTracking()
            .Where(propertyType => propertyType.IsActive)
            .OrderBy(propertyType => propertyType.SortOrder)
            .ThenBy(propertyType => propertyType.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<PropertyType>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.PropertyTypes
            .AsNoTracking()
            .OrderBy(propertyType => propertyType.SortOrder)
            .ThenBy(propertyType => propertyType.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(
        PropertyType propertyType,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(propertyType);

        await _dbContext.PropertyTypes.AddAsync(propertyType, cancellationToken);
    }

    public Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}