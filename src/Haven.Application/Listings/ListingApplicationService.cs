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

    public async Task<OwnedListingPage> GetOwnedPageAsync(
        string ownerId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(ownerId);
        ValidatePaging(page, pageSize);

        var result = await _repository.GetForOwnerAsync(
            ownerId,
            page,
            pageSize,
            cancellationToken);
        var totalPages = result.TotalCount / pageSize +
            (result.TotalCount % pageSize == 0 ? 0 : 1);

        return new OwnedListingPage(
            result.Listings.Select(OwnedListingDetails.From).ToArray(),
            page,
            pageSize,
            result.TotalCount,
            totalPages);
    }

    public async Task<PublicListingDetails?> GetPublicDetailsAsync(
        int listingId,
        CancellationToken cancellationToken = default)
    {
        if (listingId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(listingId));
        }

        var listing = await _repository.GetPublicByIdAsync(
            listingId,
            cancellationToken);

        if (listing is null || listing.Status != ListingStatus.Approved)
        {
            return null;
        }

        return await ToPublicDetailsAsync(listing, cancellationToken);
    }

    public async Task<PublicListingSearchResults> SearchPublicAsync(
        ListingSearchCriteria criteria,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(criteria);
        ValidateSearchCriteria(criteria);

        var normalizedCriteria = criteria with
        {
            Keyword = NormalizeOptional(criteria.Keyword),
            Province = NormalizeOptional(criteria.Province),
            District = NormalizeOptional(criteria.District)
        };
        var page = await _repository.SearchApprovedAsync(
            normalizedCriteria,
            cancellationToken);
        var totalPages = page.TotalCount / criteria.PageSize +
            (page.TotalCount % criteria.PageSize == 0 ? 0 : 1);

        return new PublicListingSearchResults(
            page.Listings.Select(listing => new PublicListingSummary(
                listing.Id,
                listing.PropertyTypeId,
                listing.Title,
                listing.Purpose,
                listing.PriceAmount,
                listing.AreaSqm,
                listing.Province,
                listing.District,
                listing.Bedrooms,
                listing.CreatedAtUtc)).ToArray(),
            criteria.Page,
            criteria.PageSize,
            page.TotalCount,
            totalPages);
    }

    public async Task<IReadOnlyList<PublicListingDetails>?> ComparePublicAsync(
        IReadOnlyCollection<int> listingIds,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(listingIds);

        if (listingIds.Count is < 1 or > 3)
        {
            throw new ArgumentOutOfRangeException(
                nameof(listingIds),
                "Compare requires between one and three listings.");
        }

        if (listingIds.Any(id => id <= 0) ||
            listingIds.Distinct().Count() != listingIds.Count)
        {
            throw new ArgumentException(
                "Listing IDs must be positive and unique.",
                nameof(listingIds));
        }

        var listings = await _repository.GetApprovedByIdsAsync(
            listingIds,
            cancellationToken);
        if (listings.Count != listingIds.Count)
        {
            return null;
        }

        var listingsById = listings.ToDictionary(listing => listing.Id);
        var details = new List<PublicListingDetails>(listingIds.Count);
        foreach (var listingId in listingIds)
        {
            details.Add(await ToPublicDetailsAsync(
                listingsById[listingId],
                cancellationToken));
        }

        return details;
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

    public Task<Listing?> ApproveAsync(
        int listingId,
        CancellationToken cancellationToken = default)
    {
        return ApplyModerationAsync(
            listingId,
            listing => listing.Approve(_timeProvider.GetUtcNow()),
            cancellationToken);
    }

    public Task<Listing?> RejectAsync(
        int listingId,
        string reason,
        CancellationToken cancellationToken = default)
    {
        return ApplyModerationAsync(
            listingId,
            listing => listing.Reject(reason, _timeProvider.GetUtcNow()),
            cancellationToken);
    }

    public Task<Listing?> HideAsync(
        int listingId,
        CancellationToken cancellationToken = default)
    {
        return ApplyModerationAsync(
            listingId,
            listing => listing.Hide(_timeProvider.GetUtcNow()),
            cancellationToken);
    }

    public Task<IReadOnlyList<PropertyType>> GetActivePropertyTypesAsync(
        CancellationToken cancellationToken = default)
    {
        return _propertyTypeRepository.GetActiveAsync(cancellationToken);
    }

    public async Task<ModerationListingPage> GetForAdminAsync(
        ListingStatus? status,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        if (page <= 0 ||
            pageSize is < 1 or > 100 ||
            (long)(page - 1) * pageSize > int.MaxValue)
        {
            throw new ArgumentOutOfRangeException(
                nameof(page),
                "The moderation page or page size is invalid.");
        }

        if (status is { } listingStatus &&
            !Enum.IsDefined(listingStatus))
        {
            throw new ArgumentOutOfRangeException(nameof(status));
        }

        var result = await _repository.GetForAdminAsync(
            status,
            page,
            pageSize,
            cancellationToken);
        var totalPages = result.TotalCount / pageSize +
            (result.TotalCount % pageSize == 0 ? 0 : 1);

        return new ModerationListingPage(
            result.Listings.Select(ModerationListingDetails.From).ToArray(),
            page,
            pageSize,
            result.TotalCount,
            totalPages);
    }

    public async Task<ModerationListingPage> SearchForAdminAsync(
        AdminListingCriteria criteria,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(criteria);
        ValidatePaging(criteria.Page, criteria.PageSize);
        if (criteria.Status is { } status && !Enum.IsDefined(status))
        {
            throw new ArgumentOutOfRangeException(nameof(criteria.Status));
        }

        if (criteria.PropertyTypeId is <= 0 || criteria.TagId is <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(criteria));
        }

        if (criteria.FromUtc is { } from &&
            criteria.ToUtcExclusive is { } to &&
            to <= from)
        {
            throw new ArgumentException(
                "The end date must be after the start date.",
                nameof(criteria));
        }

        var result = await _repository.SearchForAdminAsync(
            criteria with { Search = NormalizeOptional(criteria.Search) },
            cancellationToken);
        var totalPages = result.TotalCount / criteria.PageSize +
            (result.TotalCount % criteria.PageSize == 0 ? 0 : 1);
        return new ModerationListingPage(
            result.Listings.Select(ModerationListingDetails.From).ToArray(),
            criteria.Page,
            criteria.PageSize,
            result.TotalCount,
            totalPages);
    }

    public async Task<ModerationListingDetails?> GetForAdminAsync(
        int listingId,
        CancellationToken cancellationToken = default)
    {
        if (listingId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(listingId));
        }

        var listing = await _repository.GetByIdAsync(
            listingId,
            cancellationToken);
        return listing is null
            ? null
            : ModerationListingDetails.From(listing);
    }

    public async Task<bool> DeleteOwnedAsync(
        string ownerId,
        int listingId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(ownerId);
        var listing = await GetOwnedAsync(ownerId, listingId, cancellationToken);
        if (listing is null)
        {
            return false;
        }

        if (!listing.CanBeDeletedByOwner)
        {
            throw new InvalidOperationException(
                "Only draft or rejected listings can be deleted by their owner.");
        }

        await _repository.RemoveAsync(listing, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
        return true;
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

    private static void ValidateSearchCriteria(
        ListingSearchCriteria criteria)
    {
        if (criteria.Page <= 0 ||
            (long)(criteria.Page - 1) * criteria.PageSize > int.MaxValue)
        {
            throw new ArgumentOutOfRangeException(nameof(criteria.Page));
        }

        if (criteria.PageSize is < 1 or > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(criteria.PageSize));
        }

        if (criteria.PropertyTypeId is <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(criteria.PropertyTypeId));
        }

        if (criteria.Purpose is { } purpose &&
            !Enum.IsDefined(purpose))
        {
            throw new ArgumentOutOfRangeException(nameof(criteria.Purpose));
        }

        if (!Enum.IsDefined(criteria.Sort))
        {
            throw new ArgumentOutOfRangeException(nameof(criteria.Sort));
        }

        if (criteria.MinimumPrice is < 0 ||
            criteria.MaximumPrice is < 0 ||
            (criteria.MinimumPrice is { } minimumPrice &&
             criteria.MaximumPrice is { } maximumPrice &&
             minimumPrice > maximumPrice))
        {
            throw new ArgumentException(
                "The price range is invalid.",
                nameof(criteria));
        }

        if (criteria.MinimumAreaSqm is < 0 ||
            criteria.MaximumAreaSqm is < 0 ||
            (criteria.MinimumAreaSqm is { } minimumArea &&
             criteria.MaximumAreaSqm is { } maximumArea &&
             minimumArea > maximumArea))
        {
            throw new ArgumentException(
                "The area range is invalid.",
                nameof(criteria));
        }

        if (criteria.Bedrooms is < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(criteria.Bedrooms));
        }
    }

    private static void ValidatePaging(int page, int pageSize)
    {
        if (page <= 0 ||
            pageSize is < 1 or > 100 ||
            (long)(page - 1) * pageSize > int.MaxValue)
        {
            throw new ArgumentOutOfRangeException(
                nameof(page),
                "The page or page size is invalid.");
        }
    }

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private async Task<PublicListingDetails> ToPublicDetailsAsync(
        Listing listing,
        CancellationToken cancellationToken)
    {
        var propertyType = await _propertyTypeRepository.GetByIdAsync(
            listing.PropertyTypeId,
            cancellationToken);
        if (propertyType is null)
        {
            throw new InvalidOperationException(
                "The property type for this public listing was not found.");
        }

        var ownerDisplayName = await _repository.GetOwnerDisplayNameAsync(
            listing.OwnerId,
            cancellationToken);

        return new PublicListingDetails(
            listing.Id,
            listing.PropertyTypeId,
            propertyType.Name,
            listing.Title,
            listing.Description,
            listing.Purpose,
            listing.PriceAmount,
            listing.AreaSqm,
            listing.Province,
            listing.District,
            listing.AddressLine,
            listing.Bedrooms,
            listing.Bathrooms,
            listing.CreatedAtUtc,
            string.IsNullOrWhiteSpace(ownerDisplayName)
                ? null
                : ownerDisplayName);
    }

    private async Task<Listing?> ApplyModerationAsync(
        int listingId,
        Action<Listing> applyTransition,
        CancellationToken cancellationToken)
    {
        if (listingId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(listingId));
        }

        var listing = await _repository.GetByIdAsync(
            listingId,
            cancellationToken);

        if (listing is null)
        {
            return null;
        }

        applyTransition(listing);
        await _repository.SaveChangesAsync(cancellationToken);

        return listing;
    }
}
