namespace Haven.Domain.Properties;

public sealed class Tag
{
    public const int NameMaxLength = 80;
    public const int SlugMaxLength = 100;
    public const int DescriptionMaxLength = 500;

    private Tag() { }

    private Tag(
        string name,
        string normalizedName,
        string slug,
        string? description,
        DateTimeOffset nowUtc)
    {
        Name = ValidateName(name);
        NormalizedName = ValidateNormalizedName(normalizedName);
        Slug = ValidateSlug(slug);
        Description = NormalizeDescription(description);
        IsActive = true;
        CreatedAtUtc = nowUtc;
        UpdatedAtUtc = nowUtc;
    }

    public int Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string NormalizedName { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public bool IsActive { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset UpdatedAtUtc { get; private set; }
    public ICollection<ListingTag> ListingTags { get; private set; } = new List<ListingTag>();

    public static Tag Create(
        string name,
        string normalizedName,
        string slug,
        string? description,
        DateTimeOffset nowUtc) =>
        new(name, normalizedName, slug, description, nowUtc);

    public void Update(
        string name,
        string normalizedName,
        string slug,
        string? description,
        DateTimeOffset nowUtc)
    {
        Name = ValidateName(name);
        NormalizedName = ValidateNormalizedName(normalizedName);
        Slug = ValidateSlug(slug);
        Description = NormalizeDescription(description);
        UpdatedAtUtc = nowUtc;
    }

    public void Activate(DateTimeOffset nowUtc)
    {
        IsActive = true;
        UpdatedAtUtc = nowUtc;
    }

    public void Deactivate(DateTimeOffset nowUtc)
    {
        IsActive = false;
        UpdatedAtUtc = nowUtc;
    }

    private static string ValidateName(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        var value = name.Trim();
        if (value.Length > NameMaxLength)
        {
            throw new ArgumentException($"Tag name cannot exceed {NameMaxLength} characters.", nameof(name));
        }

        return value;
    }

    private static string ValidateNormalizedName(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        return value.Trim();
    }

    private static string ValidateSlug(string slug)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(slug);
        var value = slug.Trim();
        if (value.Length > SlugMaxLength)
        {
            throw new ArgumentException($"Tag slug cannot exceed {SlugMaxLength} characters.", nameof(slug));
        }

        return value;
    }

    private static string? NormalizeDescription(string? description)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            return null;
        }

        var value = description.Trim();
        if (value.Length > DescriptionMaxLength)
        {
            throw new ArgumentException($"Tag description cannot exceed {DescriptionMaxLength} characters.", nameof(description));
        }

        return value;
    }
}
