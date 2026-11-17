namespace Haven.Domain.Properties;

using System.ComponentModel.DataAnnotations;

public enum ListingPurpose
{
    [Display(Name = "Mua bán")] Sale = 1,
    [Display(Name = "Cho thuê")] Rent = 2
}
