using Haven.Application.Abstractions;

namespace Haven.Application.Locations;

public sealed class AdministrativeLocationApplicationService(IAdministrativeLocationRepository repository)
{
    public Task<IReadOnlyList<AdministrativeLocationOption>> GetProvincesAsync(CancellationToken cancellationToken = default) =>
        repository.GetProvincesAsync(cancellationToken);

    public Task<IReadOnlyList<AdministrativeLocationOption>> GetWardsAsync(string provinceCode, CancellationToken cancellationToken = default)
    {
        if (provinceCode.Length != 2 || !provinceCode.All(char.IsAsciiDigit)) throw new ArgumentException("Mã tỉnh/thành phố không hợp lệ.", nameof(provinceCode));
        return repository.GetWardsAsync(provinceCode, cancellationToken);
    }

    public Task<AdministrativeWardOption?> GetWardAsync(string wardCode, CancellationToken cancellationToken = default)
    {
        if (wardCode.Length != 5 || !wardCode.All(char.IsAsciiDigit)) throw new ArgumentException("Mã xã/phường không hợp lệ.", nameof(wardCode));
        return repository.GetWardAsync(wardCode, cancellationToken);
    }
}
