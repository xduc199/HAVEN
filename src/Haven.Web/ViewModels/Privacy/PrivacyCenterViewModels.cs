using System.ComponentModel.DataAnnotations;
using Haven.Domain.Privacy;

namespace Haven.Web.ViewModels.Privacy;

public sealed record ConsentHistoryItem(string DocumentKey, string Version, PolicyConsentAction Action, DateTimeOffset OccurredAtUtc);
public sealed record PrivacyRequestListItem(int Id, PrivacyRequestType Type, int? ListingId, string Message, PrivacyRequestStatus Status, string? Resolution, DateTimeOffset CreatedAtUtc);
public sealed record PrivacyCenterViewModel(string Email, string DisplayName, IReadOnlyList<ConsentHistoryItem> Consents, IReadOnlyList<PrivacyRequestListItem> Requests);

public sealed class CreatePrivacyRequestViewModel
{
    [Required(ErrorMessage = "Hãy chọn loại yêu cầu.")]
    public PrivacyRequestType Type { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Hãy nhập mã tin đăng hợp lệ cho phản ánh.")]
    public int? ListingId { get; set; }

    [Required(ErrorMessage = "Hãy mô tả nội dung yêu cầu.")]
    [StringLength(3000, MinimumLength = 1, ErrorMessage = "Nội dung yêu cầu tối đa 3.000 ký tự.")]
    public string Message { get; set; } = string.Empty;
}

public sealed class UpdatePrivacyRequestViewModel
{
    public int Id { get; set; }
    [Required]
    public PrivacyRequestStatus Status { get; set; }
    [StringLength(1500, ErrorMessage = "Kết quả xử lý tối đa 1.500 ký tự.")]
    public string? Resolution { get; set; }
}

public sealed record AdminPrivacyRequestListItem(int Id, string UserName, string Email, PrivacyRequestType Type, int? ListingId, string Message, PrivacyRequestStatus Status, DateTimeOffset CreatedAtUtc);
