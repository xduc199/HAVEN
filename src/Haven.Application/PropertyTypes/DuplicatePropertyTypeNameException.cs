namespace Haven.Application.PropertyTypes;

public sealed class DuplicatePropertyTypeNameException(string name)
    : Exception($"A property type named '{name}' already exists.");
