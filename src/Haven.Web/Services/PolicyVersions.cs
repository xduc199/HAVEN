namespace Haven.Web.Services;

public static class PolicyVersions
{
    public const string Terms = "1.0.0";
    public const string Privacy = "1.0.0";
    public static readonly DateOnly EffectiveDate = new(2026, 11, 17);

    public static string CurrentFor(string key) => key switch
    {
        "Terms" => Terms,
        "Privacy" => Privacy,
        "PostingRules" => "1.0.0",
        "Complaints" => "1.0.0",
        "CookiePolicy" => "1.0.0",
        _ => throw new ArgumentOutOfRangeException(nameof(key))
    };
}
