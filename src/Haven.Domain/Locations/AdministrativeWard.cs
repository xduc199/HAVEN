namespace Haven.Domain.Locations;

public sealed class AdministrativeWard
{
    private AdministrativeWard() { }
    private AdministrativeWard(string code, string provinceCode, string name, string type)
    {
        if (code.Length != 5 || !code.All(char.IsAsciiDigit)) throw new ArgumentException("Ward code must contain five digits.", nameof(code));
        if (provinceCode.Length != 2 || !provinceCode.All(char.IsAsciiDigit)) throw new ArgumentException("Province code must contain two digits.", nameof(provinceCode));
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        if (type is not ("Xã" or "Phường" or "Đặc khu")) throw new ArgumentException("Unknown administrative unit type.", nameof(type));
        Code = code;
        ProvinceCode = provinceCode;
        Name = name.Trim();
        Type = type;
    }
    public string Code { get; private set; } = string.Empty;
    public string ProvinceCode { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string Type { get; private set; } = string.Empty;
    public AdministrativeProvince Province { get; private set; } = null!;
    public static AdministrativeWard Create(string code, string provinceCode, string name, string type) => new(code, provinceCode, name, type);
}
