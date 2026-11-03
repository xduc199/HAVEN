using System.Net.Mail;
using System.Text.RegularExpressions;

namespace Haven.Domain.Consultations;

public sealed class ConsultationRequest
{
    public const int RequesterIdMaxLength = 450;
    public const int NameMaxLength = 100;
    public const int EmailMaxLength = 256;
    public const int PhoneNumberMaxLength = 32;
    public const int MessageMaxLength = 2000;

    private ConsultationRequest()
    {
    }

    private ConsultationRequest(
        string requesterId,
        string contactName,
        string email,
        string phoneNumber,
        int? listingId,
        string? message,
        DateTimeOffset nowUtc)
    {
        ValidateRequired(requesterId, contactName, email, phoneNumber);
        ValidateEmail(email);
        ValidatePhoneNumber(phoneNumber);
        ValidateListingId(listingId);

        RequesterId = requesterId.Trim();
        ContactName = contactName.Trim();
        Email = email.Trim();
        PhoneNumber = phoneNumber.Trim();
        ListingId = listingId;
        Message = NormalizeMessage(message);
        Status = ConsultationStatus.New;
        CreatedAtUtc = nowUtc;
        UpdatedAtUtc = nowUtc;
    }

    public int Id { get; private set; }

    public string RequesterId { get; private set; } = string.Empty;

    public string ContactName { get; private set; } = string.Empty;

    public string Email { get; private set; } = string.Empty;

    public string PhoneNumber { get; private set; } = string.Empty;

    public int? ListingId { get; private set; }

    public string? Message { get; private set; }

    public ConsultationStatus Status { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public static ConsultationRequest Create(
        string requesterId,
        string contactName,
        string email,
        string phoneNumber,
        int? listingId,
        string? message,
        DateTimeOffset nowUtc) =>
        new(
            requesterId,
            contactName,
            email,
            phoneNumber,
            listingId,
            message,
            nowUtc);

    public void ChangeStatus(ConsultationStatus status, DateTimeOffset nowUtc)
    {
        if (!Enum.IsDefined(status))
        {
            throw new ArgumentOutOfRangeException(nameof(status));
        }

        Status = status;
        UpdatedAtUtc = nowUtc;
    }

    private static void ValidateRequired(
        string requesterId,
        string contactName,
        string email,
        string phoneNumber)
    {
        ValidateText(requesterId, RequesterIdMaxLength, nameof(requesterId));
        ValidateText(contactName, NameMaxLength, nameof(contactName));
        ValidateText(email, EmailMaxLength, nameof(email));
        ValidateText(phoneNumber, PhoneNumberMaxLength, nameof(phoneNumber));
    }

    private static void ValidateText(string value, int maxLength, string name)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("A value is required.", name);
        }

        if (value.Trim().Length > maxLength)
        {
            throw new ArgumentException(
                $"The value cannot exceed {maxLength} characters.",
                name);
        }
    }

    private static void ValidateEmail(string email)
    {
        if (!MailAddress.TryCreate(email.Trim(), out var address) ||
            !string.Equals(
                address.Address,
                email.Trim(),
                StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("A valid email is required.", nameof(email));
        }
    }

    private static void ValidatePhoneNumber(string phoneNumber)
    {
        var trimmed = phoneNumber.Trim();
        var digits = trimmed.Count(char.IsDigit);
        if (digits is < 7 or > 15 ||
            !Regex.IsMatch(trimmed, @"^\+?[0-9\s().-]+$"))
        {
            throw new ArgumentException(
                "A valid phone number is required.",
                nameof(phoneNumber));
        }
    }

    private static void ValidateListingId(int? listingId)
    {
        if (listingId is <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(listingId));
        }
    }

    private static string? NormalizeMessage(string? message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return null;
        }

        var normalized = message.Trim();
        if (normalized.Length > MessageMaxLength)
        {
            throw new ArgumentException(
                $"The message cannot exceed {MessageMaxLength} characters.",
                nameof(message));
        }

        return normalized;
    }
}
