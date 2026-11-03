using System.ComponentModel.DataAnnotations;

namespace Haven.Web.Models.PropertyTypes;

public sealed record PropertyTypeWriteRequest(
    [param: Required]
    [param: StringLength(100, MinimumLength = 1)]
    string Name,
    [param: Range(0, int.MaxValue)] int SortOrder,
    bool IsActive = true);
