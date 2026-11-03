namespace Haven.Application.Consultations;

public sealed class ConsultationListingNotFoundException(int listingId)
    : Exception($"Approved listing {listingId} was not found.");
