using Haven.Application.Abstractions;
using Haven.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Haven.Infrastructure.Repositories;

public sealed class EfAdministrativeLocationRepository(ApplicationDbContext db) : IAdministrativeLocationRepository
{
    public async Task<IReadOnlyList<AdministrativeLocationOption>> GetProvincesAsync(CancellationToken cancellationToken = default) =>
        await db.AdministrativeProvinces.AsNoTracking().OrderBy(province => province.Name)
            .Select(province => new AdministrativeLocationOption(province.Code, province.Name)).ToArrayAsync(cancellationToken);

    public async Task<IReadOnlyList<AdministrativeLocationOption>> GetWardsAsync(string provinceCode, CancellationToken cancellationToken = default) =>
        await db.AdministrativeWards.AsNoTracking().Where(ward => ward.ProvinceCode == provinceCode).OrderBy(ward => ward.Name)
            .Select(ward => new AdministrativeLocationOption(ward.Code, ward.Name)).ToArrayAsync(cancellationToken);

    public Task<AdministrativeWardOption?> GetWardAsync(string wardCode, CancellationToken cancellationToken = default) =>
        db.AdministrativeWards.AsNoTracking().Where(ward => ward.Code == wardCode)
            .Select(ward => new AdministrativeWardOption(ward.Code, ward.ProvinceCode, ward.Province.Name, ward.Name, ward.Type))
            .SingleOrDefaultAsync(cancellationToken);
}
