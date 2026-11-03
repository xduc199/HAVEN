using System.ComponentModel.DataAnnotations;

namespace Haven.Web.Models.Auth;

public sealed record UpdateDisplayNameRequest(
    [Required]
    [StringLength(100, MinimumLength = 1)]
    string DisplayName);
