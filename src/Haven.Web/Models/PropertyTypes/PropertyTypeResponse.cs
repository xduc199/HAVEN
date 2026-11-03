using Haven.Domain.Properties;

namespace Haven.Web.Models.PropertyTypes;

public sealed record PropertyTypeResponse(
    int Id,
    string Name,
    bool IsActive,
    int SortOrder)
{
    public static PropertyTypeResponse From(PropertyType propertyType)
    {
        ArgumentNullException.ThrowIfNull(propertyType);

        return new PropertyTypeResponse(
            propertyType.Id,
            propertyType.Name,
            propertyType.IsActive,
            propertyType.SortOrder);
    }
}
