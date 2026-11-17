using Microsoft.AspNetCore.Identity;

namespace Haven.Web.Services;

public sealed class VietnameseIdentityErrorDescriber : IdentityErrorDescriber
{
    public override IdentityError DuplicateEmail(string email) => Error(nameof(DuplicateEmail), "Email đã được sử dụng.");
    public override IdentityError DuplicateUserName(string userName) => Error(nameof(DuplicateUserName), "Tên đăng nhập đã được sử dụng.");
    public override IdentityError InvalidEmail(string? email) => Error(nameof(InvalidEmail), "Email chưa đúng định dạng.");
    public override IdentityError InvalidUserName(string? userName) => Error(nameof(InvalidUserName), "Tên đăng nhập không hợp lệ.");
    public override IdentityError PasswordTooShort(int length) => Error(nameof(PasswordTooShort), $"Mật khẩu phải có ít nhất {length} ký tự.");
    public override IdentityError PasswordRequiresDigit() => Error(nameof(PasswordRequiresDigit), "Mật khẩu phải có ít nhất một chữ số.");
    public override IdentityError PasswordRequiresLower() => Error(nameof(PasswordRequiresLower), "Mật khẩu phải có ít nhất một chữ thường.");
    public override IdentityError PasswordRequiresUpper() => Error(nameof(PasswordRequiresUpper), "Mật khẩu phải có ít nhất một chữ hoa.");
    public override IdentityError PasswordRequiresNonAlphanumeric() => Error(nameof(PasswordRequiresNonAlphanumeric), "Mật khẩu phải có ít nhất một ký tự đặc biệt.");
    public override IdentityError PasswordRequiresUniqueChars(int uniqueChars) => Error(nameof(PasswordRequiresUniqueChars), $"Mật khẩu phải có ít nhất {uniqueChars} ký tự khác nhau.");
    public override IdentityError ConcurrencyFailure() => Error(nameof(ConcurrencyFailure), "Dữ liệu vừa được thay đổi. Hãy tải lại trang và thử lại.");

    private static IdentityError Error(string code, string description) => new() { Code = code, Description = description };
}
