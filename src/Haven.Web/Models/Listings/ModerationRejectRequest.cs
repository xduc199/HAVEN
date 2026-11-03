using System.ComponentModel.DataAnnotations;
using Haven.Domain.Properties;

namespace Haven.Web.Models.Listings;

public sealed record ModerationRejectRequest(
    [param: Required]
    [param: StringLength(Listing.RejectionReasonMaxLength, MinimumLength = 1)]
    string Reason);
