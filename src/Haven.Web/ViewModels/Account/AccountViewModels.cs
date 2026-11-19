using System.ComponentModel.DataAnnotations;

namespace Haven.Web.ViewModels.Account;

public sealed class RegisterViewModel
{
    [Required(ErrorMessage = "Hãy nhập tên hiển thị.")]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "Tên hiển thị không được vượt quá 100 ký tự.")]
    public string DisplayName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Hãy nhập email.")]
    [EmailAddress(ErrorMessage = "Email chưa đúng định dạng.")]
    [StringLength(256, ErrorMessage = "Email không được vượt quá 256 ký tự.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Hãy nhập mật khẩu.")]
    [StringLength(256, MinimumLength = 12, ErrorMessage = "Mật khẩu phải có từ 12 đến 256 ký tự.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Hãy xác nhận mật khẩu.")]
    [Compare(nameof(Password), ErrorMessage = "Mật khẩu xác nhận không khớp.")]
    [DataType(DataType.Password)]
    public string ConfirmPassword { get; set; } = string.Empty;

    public bool AcceptedTerms { get; set; }
    public bool ConsentedPersonalDataProcessing { get; set; }
    public string TermsVersion { get; set; } = Haven.Web.Services.PolicyVersions.Terms;
    public string PrivacyVersion { get; set; } = Haven.Web.Services.PolicyVersions.Privacy;
}

public sealed class LoginViewModel
{
    [Required(ErrorMessage = "Hãy nhập email.")]
    [EmailAddress(ErrorMessage = "Email chưa đúng định dạng.")]
    [StringLength(256)]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Hãy nhập mật khẩu.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    public bool RememberMe { get; set; }

    public string? ReturnUrl { get; set; }
}

public sealed class ProfileViewModel
{
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string DisplayName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? AvatarPath { get; set; }
}

public sealed class ChangePasswordViewModel
{
    [Required]
    [DataType(DataType.Password)]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required]
    [StringLength(256, MinimumLength = 12)]
    [DataType(DataType.Password)]
    public string NewPassword { get; set; } = string.Empty;

    [Required]
    [Compare(nameof(NewPassword))]
    [DataType(DataType.Password)]
    public string ConfirmPassword { get; set; } = string.Empty;
}
