namespace Haven.Domain.Properties;

public sealed class PropertyType
{
    private PropertyType() { }

    private PropertyType(string name, int sortOrder)
    {
        ValidateName(name);

        if (sortOrder < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(sortOrder));
        }

        Name = name.Trim();
        SortOrder = sortOrder;
        IsActive = true;
    }

    public int Id { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public bool IsActive { get; private set; }

    public int SortOrder { get; private set; }

    public static PropertyType Create(string name, int sortOrder = 0)
        => new(name, sortOrder);

    public void Rename(string name)
    {
        ValidateName(name);
        Name = name.Trim();
    }

    public void ChangeSortOrder(int sortOrder)
    {
        if (sortOrder < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(sortOrder));
        }

        SortOrder = sortOrder;
    }

    public void Activate() => IsActive = true;

    public void Deactivate() => IsActive = false;

    private static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(
                "Property type name is required.",
                nameof(name));
        }

        if (name.Trim().Length > 100)
        {
            throw new ArgumentException(
                "Property type name cannot exceed 100 characters.",
                nameof(name));
        }
    }
}