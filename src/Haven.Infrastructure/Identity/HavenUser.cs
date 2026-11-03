using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Haven.Infrastructure.Identity;

public sealed class HavenUser : IdentityUser
{
    public const int DisplayNameMaxLength = 100;

    [MaxLength(DisplayNameMaxLength)]
    public string DisplayName { get; set; } = string.Empty;
}
