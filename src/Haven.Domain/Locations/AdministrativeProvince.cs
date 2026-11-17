namespace Haven.Domain.Locations;

public sealed class AdministrativeProvince
{
    private AdministrativeProvince() { }
    private AdministrativeProvince(string code, string name)
    {
        if (code.Length != 2 || !code.All(char.IsAsciiDigit)) throw new ArgumentException("Province code must contain two digits.", nameof(code));
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Code = code;
        Name = name.Trim();
    }
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public static AdministrativeProvince Create(string code, string name) => new(code, name);
}
