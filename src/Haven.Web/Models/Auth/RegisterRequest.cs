using System.ComponentModel.DataAnnotations;

namespace Haven.Web.Models.Auth;

public sealed record RegisterRequest(
    [Required(ErrorMessage = "Hãy nhập tên hiển thị.")]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "Tên hiển thị không được vượt quá 100 ký tự.")]
    string DisplayName,
    [Required(ErrorMessage = "Hãy nhập email.")]
    [EmailAddress(ErrorMessage = "Email chưa đúng định dạng.")]
    [StringLength(256, ErrorMessage = "Email không được vượt quá 256 ký tự.")]
    string Email,
    [Required(ErrorMessage = "Hãy nhập mật khẩu.")]
    [StringLength(256, MinimumLength = 12, ErrorMessage = "Mật khẩu phải có từ 12 đến 256 ký tự.")]
    string Password,
    [Required(ErrorMessage = "Bạn cần đồng ý với Điều khoản sử dụng.")]
    bool AcceptedTerms,
    [Required(ErrorMessage = "Bạn cần xác nhận việc xử lý dữ liệu cá nhân.")]
    bool ConsentedPersonalDataProcessing,
    [Required(ErrorMessage = "Thiếu phiên bản Điều khoản sử dụng.")]
    string TermsVersion,
    [Required(ErrorMessage = "Thiếu phiên bản Chính sách quyền riêng tư.")]
    string PrivacyVersion);
