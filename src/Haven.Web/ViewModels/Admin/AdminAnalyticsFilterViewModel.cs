using System.ComponentModel.DataAnnotations;

namespace Haven.Web.ViewModels.Admin;

public sealed class AdminAnalyticsFilterViewModel
{
    [Required]
    [RegularExpression("^(7d|30d|90d|ytd|custom)$")]
    public string Period { get; set; } = "7d";

    public DateOnly? FromDate { get; set; }

    public DateOnly? ToDate { get; set; }

    public (DateOnly FromDate, DateOnly ToDate) Resolve(DateOnly today)
    {
        return Period switch
        {
            "7d" => (today.AddDays(-6), today),
            "30d" => (today.AddDays(-29), today),
            "90d" => (today.AddDays(-89), today),
            "ytd" => (new DateOnly(today.Year, 1, 1), today),
            "custom" when FromDate.HasValue && ToDate.HasValue =>
                (FromDate.Value, ToDate.Value),
            "custom" => throw new ArgumentException("Choose both dates for a custom range."),
            _ => throw new ArgumentException("Select a supported reporting period.")
        };
    }
}
