namespace Haven.Domain.Privacy;

public sealed class PrivacyRequest
{
    public const int UserIdMaxLength = 450;
    public const int MessageMaxLength = 3000;
    public const int ResolutionMaxLength = 1500;

    private PrivacyRequest() { }

    public int Id { get; private set; }
    public string UserId { get; private set; } = string.Empty;
    public PrivacyRequestType Type { get; private set; }
    public int? ListingId { get; private set; }
    public string Message { get; private set; } = string.Empty;
    public PrivacyRequestStatus Status { get; private set; }
    public string? Resolution { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public static PrivacyRequest Create(string userId, PrivacyRequestType type, string message,
        int? listingId, DateTimeOffset nowUtc)
    {
        if (string.IsNullOrWhiteSpace(userId) || userId.Length > UserIdMaxLength)
            throw new ArgumentException("Mã tài khoản không hợp lệ.", nameof(userId));
        if (!Enum.IsDefined(type)) throw new ArgumentOutOfRangeException(nameof(type));
        if (listingId is <= 0)
            throw new ArgumentException("Mã tin đăng không hợp lệ.", nameof(listingId));
        if (type != PrivacyRequestType.Complaint && listingId is not null)
            throw new ArgumentException("Chỉ phản ánh tin đăng mới được liên kết với tin.", nameof(listingId));
        if (string.IsNullOrWhiteSpace(message) || message.Trim().Length > MessageMaxLength)
            throw new ArgumentException("Nội dung yêu cầu phải có từ 1 đến 3.000 ký tự.", nameof(message));

        var now = nowUtc.ToUniversalTime();
        return new PrivacyRequest
        {
            UserId = userId,
            Type = type,
            ListingId = listingId,
            Message = message.Trim(),
            Status = PrivacyRequestStatus.New,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };
    }

    public void UpdateStatus(PrivacyRequestStatus status, string? resolution, DateTimeOffset nowUtc)
    {
        if (Status is PrivacyRequestStatus.Completed or PrivacyRequestStatus.Declined)
            throw new InvalidOperationException("Yêu cầu đã kết thúc và không thể thay đổi trạng thái.");
        if (status is not (PrivacyRequestStatus.InProgress or PrivacyRequestStatus.Completed or PrivacyRequestStatus.Declined))
            throw new ArgumentOutOfRangeException(nameof(status));
        if ((status is PrivacyRequestStatus.Completed or PrivacyRequestStatus.Declined) && string.IsNullOrWhiteSpace(resolution))
            throw new ArgumentException("Cần ghi kết quả xử lý khi kết thúc yêu cầu.", nameof(resolution));
        if (resolution?.Length > ResolutionMaxLength)
            throw new ArgumentException("Kết quả xử lý không được vượt quá 1.500 ký tự.", nameof(resolution));

        Status = status;
        Resolution = string.IsNullOrWhiteSpace(resolution) ? null : resolution.Trim();
        UpdatedAtUtc = nowUtc.ToUniversalTime();
    }
}
