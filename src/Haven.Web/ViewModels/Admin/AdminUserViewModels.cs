using System.ComponentModel.DataAnnotations;

namespace Haven.Web.ViewModels.Admin;

public sealed class AdminUserFilterViewModel
{
    [StringLength(200)]
    public string? Search { get; set; }

    [Range(1, int.MaxValue)]
    public int Page { get; set; } = 1;
}

public sealed record AdminUserRowViewModel(
    string Id,
    string DisplayName,
    string Email,
    string Roles,
    bool IsAdminLocked,
    bool IsTemporarilyLocked,
    DateTimeOffset? TemporaryLockoutEnd,
    bool IsAdmin,
    string? AvatarPath = null);

public sealed record AdminUserPageViewModel(
    IReadOnlyList<AdminUserRowViewModel> Items,
    string? Search,
    int Page,
    int PageSize,
    int TotalCount)
{
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
}
