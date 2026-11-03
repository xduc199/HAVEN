namespace Haven.Web.Models.Auth;

public sealed record AccountResponse(
    string Id,
    string DisplayName,
    string Email,
    IReadOnlyList<string> Roles);
