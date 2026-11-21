namespace Haven.Web.Services;

public static class PolicyVersions
{
    public const string Terms = "1.1.0";
    public const string Privacy = "1.1.0";
    public const string PostingRules = "1.1.0";
    public const string Complaints = "1.1.0";
    public const string CookiePolicy = "1.1.0";
    public static readonly DateOnly EffectiveDate = new(2026, 11, 21);

    public static string CurrentFor(string key) => key switch
    {
        "Terms" => Terms,
        "Privacy" => Privacy,
        "PostingRules" => PostingRules,
        "Complaints" => Complaints,
        "CookiePolicy" => CookiePolicy,
        _ => throw new ArgumentOutOfRangeException(nameof(key))
    };
}
