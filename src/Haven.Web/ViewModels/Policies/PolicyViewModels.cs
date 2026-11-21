namespace Haven.Web.ViewModels.Policies;

public sealed record PolicySectionViewModel(string Heading, IReadOnlyList<string> Paragraphs);

public sealed record PolicyPageViewModel(
    string Key,
    string Title,
    string Summary,
    string Version,
    DateOnly EffectiveDate,
    IReadOnlyList<PolicySectionViewModel> Sections,
    IReadOnlyList<(string Key, string Title)> OtherPolicies,
    string OperatorName,
    string SupportEmail,
    string SupportPhone,
    string SupportAddress)
{
    // The first published edition was created and made effective on the same date.
    public DateOnly LastUpdated => EffectiveDate;
}
