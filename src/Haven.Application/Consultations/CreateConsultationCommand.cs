namespace Haven.Application.Consultations;

public sealed record CreateConsultationCommand(
    string ContactName,
    string Email,
    string PhoneNumber,
    int? ListingId,
    string? Message);
