using System.ComponentModel.DataAnnotations;

namespace Haven.Web.Models.Auth;

public sealed record RegisterRequest(
    [Required]
    [StringLength(100, MinimumLength = 1)]
    string DisplayName,
    [Required]
    [EmailAddress]
    [StringLength(256)]
    string Email,
    [Required]
    [StringLength(256, MinimumLength = 12)]
    string Password);
