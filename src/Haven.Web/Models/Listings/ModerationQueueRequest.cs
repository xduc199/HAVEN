using System.ComponentModel.DataAnnotations;

namespace Haven.Web.Models.Listings;

public sealed record ModerationQueueRequest(
    [param: Range(1, int.MaxValue)] int Page = 1,
    [param: Range(1, 100)] int PageSize = 20);
