using Haven.Application.Abstractions;
using Haven.Domain.Properties;

namespace Haven.Application.Listings;

public sealed class ListingApplicationService
{
    private readonly IListingRepository _repository;
    private readonly IPropertyTypeRepository _propertyTypeRepository;
    private readonly TimeProvider _timeProvider;

    public ListingApplicationService(
        IListingRepository repository,
        IPropertyTypeRepository propertyTypeRepository,
        TimeProvider timeProvider)
    {
        _repository = repository;
        _propertyTypeRepository = propertyTypeRepository;
        _timeProvider = timeProvider;
    }

    public async Task<Listing> CreateAsync(
        string ownerId,
        CreateListingCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(ownerId);
        ArgumentNullException.ThrowIfNull(command);

        await EnsureActivePropertyTypeAsync(
            command.PropertyTypeId,
            cancellationToken);

        var nowUtc = _timeProvider.GetUtcNow();

        var listing = Listing.Create(
            ownerId: ownerId,
            propertyTypeId: command.PropertyTypeId,
            title: command.Title,
            description: command.Description,
            purpose: command.Purpose,
            priceAmount: command.PriceAmount,
            areaSqm: command.AreaSqm,
            province: command.Province,
            district: command.District,
            addressLine: command.AddressLine,
            nowUtc: nowUtc,
            bedrooms: command.Bedrooms,
            bathrooms: command.Bathrooms);

        await _repository.AddAsync(listing, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return listing;
    }

    public async Task<Listing?> GetOwnedAsync(
        string ownerId,
        int listingId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(ownerId);

        if (listingId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(listingId));
        }

        var listing = await _repository.GetByIdAsync(
            listingId,
            cancellationToken);

        if (listing is null ||
            !string.Equals(
                listing.OwnerId,
                ownerId,
                StringComparison.Ordinal))
        {
            return null;
        }

        return listing;
    }

    public async Task<Listing?> UpdateAsync(
        string ownerId,
        int listingId,
        UpdateListingCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(ownerId);
        ArgumentNullException.ThrowIfNull(command);

        var listing = await GetOwnedAsync(
            ownerId,
            listingId,
            cancellationToken);

        if (listing is null)
        {
            return null;
        }

        await EnsureActivePropertyTypeAsync(
            command.PropertyTypeId,
            cancellationToken);

        listing.UpdateCoreDetails(
            propertyTypeId: command.PropertyTypeId,
            title: command.Title,
            description: command.Description,
            purpose: command.Purpose,
            priceAmount: command.PriceAmount,
            areaSqm: command.AreaSqm,
            province: command.Province,
            district: command.District,
            addressLine: command.AddressLine,
            nowUtc: _timeProvider.GetUtcNow(),
            bedrooms: command.Bedrooms,
            bathrooms: command.Bathrooms);

        await _repository.SaveChangesAsync(cancellationToken);

        return listing;
    }

    public async Task<Listing?> SubmitForReviewAsync(
        string ownerId,
        int listingId,
        CancellationToken cancellationToken = default)
    {
        var listing = await GetOwnedAsync(
            ownerId,
            listingId,
            cancellationToken);

        if (listing is null)
        {
            return null;
        }

        listing.SubmitForReview(_timeProvider.GetUtcNow());

        await _repository.SaveChangesAsync(cancellationToken);

        return listing;
    }

    public Task<IReadOnlyList<PropertyType>> GetActivePropertyTypesAsync(
        CancellationToken cancellationToken = default)
    {
        return _propertyTypeRepository.GetActiveAsync(cancellationToken);
    }

    private async Task EnsureActivePropertyTypeAsync(
        int propertyTypeId,
        CancellationToken cancellationToken)
    {
        var propertyType = await _propertyTypeRepository.GetByIdAsync(
            propertyTypeId,
            cancellationToken);

        if (propertyType is null || !propertyType.IsActive)
        {
            throw new ArgumentException(
                "The selected property type does not exist or is inactive.",
                nameof(propertyTypeId));
        }
    }
}