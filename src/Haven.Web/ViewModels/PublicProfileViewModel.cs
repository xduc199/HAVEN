using Haven.Web.ViewModels.Properties;

namespace Haven.Web.ViewModels;

public sealed record PublicProfileViewModel(
    string DisplayName,
    string? AvatarPath,
    IReadOnlyList<PropertyCardViewModel> Listings,
    int Page,
    int PageSize,
    long TotalCount)
{
    public long TotalPages => TotalCount == 0 ? 0 : (TotalCount + PageSize - 1) / PageSize;
}
