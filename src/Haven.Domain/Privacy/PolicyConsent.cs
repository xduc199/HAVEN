namespace Haven.Domain.Privacy;

public sealed class PolicyConsent
{
    public const int UserIdMaxLength = 450;
    public const int DocumentKeyMaxLength = 40;
    public const int VersionMaxLength = 32;
    public const int SourceMaxLength = 32;

    private PolicyConsent() { }

    public int Id { get; private set; }
    public string UserId { get; private set; } = string.Empty;
    public string DocumentKey { get; private set; } = string.Empty;
    public string DocumentVersion { get; private set; } = string.Empty;
    public PolicyConsentAction Action { get; private set; }
    public string Source { get; private set; } = string.Empty;
    public DateTimeOffset OccurredAtUtc { get; private set; }

    public static PolicyConsent Record(string userId, string documentKey, string documentVersion,
        PolicyConsentAction action, string source, DateTimeOffset occurredAtUtc)
    {
        if (string.IsNullOrWhiteSpace(userId) || userId.Length > UserIdMaxLength)
            throw new ArgumentException("Mã tài khoản không hợp lệ.", nameof(userId));
        if (string.IsNullOrWhiteSpace(documentKey) || documentKey.Length > DocumentKeyMaxLength)
            throw new ArgumentException("Mã chính sách không hợp lệ.", nameof(documentKey));
        if (string.IsNullOrWhiteSpace(documentVersion) || documentVersion.Length > VersionMaxLength)
            throw new ArgumentException("Phiên bản chính sách không hợp lệ.", nameof(documentVersion));
        if (string.IsNullOrWhiteSpace(source) || source.Length > SourceMaxLength)
            throw new ArgumentException("Nguồn ghi nhận không hợp lệ.", nameof(source));
        if (!Enum.IsDefined(action)) throw new ArgumentOutOfRangeException(nameof(action));

        return new PolicyConsent
        {
            UserId = userId,
            DocumentKey = documentKey,
            DocumentVersion = documentVersion,
            Action = action,
            Source = source,
            OccurredAtUtc = occurredAtUtc.ToUniversalTime()
        };
    }
}
