using System.ComponentModel.DataAnnotations;

namespace Haven.Web.Models.Auth;

public sealed record LoginRequest(
    [Required]
    [EmailAddress]
    [StringLength(256)]
    string Email,
    [Required]
    [StringLength(256, MinimumLength = 1)]
    string Password,
    bool RememberMe = false);
