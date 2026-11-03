using System.ComponentModel.DataAnnotations;

namespace Haven.Web.Models.Auth;

public sealed record ChangePasswordRequest(
    [Required]
    [StringLength(256, MinimumLength = 1)]
    string CurrentPassword,
    [Required]
    [StringLength(256, MinimumLength = 12)]
    string NewPassword);
