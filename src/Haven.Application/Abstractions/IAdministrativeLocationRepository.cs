namespace Haven.Application.Abstractions;

public interface IAdministrativeLocationRepository
{
    Task<IReadOnlyList<AdministrativeLocationOption>> GetProvincesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AdministrativeLocationOption>> GetWardsAsync(string provinceCode, CancellationToken cancellationToken = default);
    Task<AdministrativeWardOption?> GetWardAsync(string wardCode, CancellationToken cancellationToken = default);
}

public sealed record AdministrativeLocationOption(string Code, string Name);
public sealed record AdministrativeWardOption(string Code, string ProvinceCode, string ProvinceName, string Name, string Type);
