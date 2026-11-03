using System.ComponentModel.DataAnnotations;

namespace Haven.Web.Models.Listings;

public sealed class ListingCompareRequest
{
    [MinLength(1)]
    [MaxLength(3)]
    public int[] Ids { get; init; } = [];
}
