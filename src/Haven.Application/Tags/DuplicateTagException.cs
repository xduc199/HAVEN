namespace Haven.Application.Tags;

public sealed class DuplicateTagException(string value)
    : InvalidOperationException($"A tag with the name or slug '{value}' already exists.");
