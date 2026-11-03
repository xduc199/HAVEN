using Haven.Domain.Consultations;

namespace Haven.Application.Consultations;

public sealed record ConsultationDetails(
    int Id,
    string ContactName,
    string Email,
    string PhoneNumber,
    int? ListingId,
    string? Message,
    ConsultationStatus Status,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc)
{
    public static ConsultationDetails From(ConsultationRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        return new ConsultationDetails(
            request.Id,
            request.ContactName,
            request.Email,
            request.PhoneNumber,
            request.ListingId,
            request.Message,
            request.Status,
            request.CreatedAtUtc,
            request.UpdatedAtUtc);
    }
}
