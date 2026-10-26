namespace Haven.Domain.Properties;

public sealed class Listing
{
    private Listing()
    {
    }

    private Listing(
        string ownerId,
        int propertyTypeId,
        string title,
        string description,
        ListingPurpose purpose,
        decimal priceAmount,
        decimal areaSqm,
        string province,
        string district,
        string addressLine,
        DateTimeOffset nowUtc,
        int? bedrooms,
        int? bathrooms)
    {
        ValidateRequiredFields(
            ownerId,
            propertyTypeId,
            title,
            province,
            district,
            addressLine);

        ValidatePriceAndArea(priceAmount, areaSqm);
        ValidateRooms(bedrooms, bathrooms);
        ValidatePurpose(purpose);
        ArgumentNullException.ThrowIfNull(description);

        OwnerId = ownerId.Trim();
        PropertyTypeId = propertyTypeId;
        Title = title.Trim();
        Description = description.Trim();
        Purpose = purpose;
        PriceAmount = priceAmount;
        AreaSqm = areaSqm;
        Province = province.Trim();
        District = district.Trim();
        AddressLine = addressLine.Trim();
        Bedrooms = bedrooms;
        Bathrooms = bathrooms;

        Status = ListingStatus.Draft;
        CreatedAtUtc = nowUtc;
        UpdatedAtUtc = nowUtc;
    }

    public int Id { get; private set; }

    public string OwnerId { get; private set; } = string.Empty;

    public int PropertyTypeId { get; private set; }

    public string Title { get; private set; } = string.Empty;

    public string Description { get; private set; } = string.Empty;

    public ListingPurpose Purpose { get; private set; }

    public decimal PriceAmount { get; private set; }

    public decimal AreaSqm { get; private set; }

    public string Province { get; private set; } = string.Empty;

    public string District { get; private set; } = string.Empty;

    public string AddressLine { get; private set; } = string.Empty;

    public int? Bedrooms { get; private set; }

    public int? Bathrooms { get; private set; }

    public ListingStatus Status { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public DateTimeOffset? SubmittedAtUtc { get; private set; }

    public DateTimeOffset? ApprovedAtUtc { get; private set; }

    public string? RejectionReason { get; private set; }

    public static Listing Create(
        string ownerId,
        int propertyTypeId,
        string title,
        string description,
        ListingPurpose purpose,
        decimal priceAmount,
        decimal areaSqm,
        string province,
        string district,
        string addressLine,
        DateTimeOffset nowUtc,
        int? bedrooms = null,
        int? bathrooms = null)
    {
        return new Listing(
            ownerId,
            propertyTypeId,
            title,
            description,
            purpose,
            priceAmount,
            areaSqm,
            province,
            district,
            addressLine,
            nowUtc,
            bedrooms,
            bathrooms);
    }

    public void SubmitForReview(DateTimeOffset nowUtc)
    {
        if (Status is not ListingStatus.Draft
            and not ListingStatus.Rejected)
        {
            throw new InvalidOperationException(
                "Only draft or rejected listings can be submitted.");
        }

        Status = ListingStatus.Pending;
        SubmittedAtUtc = nowUtc;
        ApprovedAtUtc = null;
        RejectionReason = null;
        UpdatedAtUtc = nowUtc;
    }

    public void Approve(DateTimeOffset nowUtc)
    {
        if (Status != ListingStatus.Pending)
        {
            throw new InvalidOperationException(
                "Only pending listings can be approved.");
        }

        Status = ListingStatus.Approved;
        ApprovedAtUtc = nowUtc;
        RejectionReason = null;
        UpdatedAtUtc = nowUtc;
    }

    public void Reject(string reason, DateTimeOffset nowUtc)
    {
        if (Status != ListingStatus.Pending)
        {
            throw new InvalidOperationException(
                "Only pending listings can be rejected.");
        }

        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new ArgumentException(
                "A rejection reason is required.",
                nameof(reason));
        }

        Status = ListingStatus.Rejected;
        RejectionReason = reason.Trim();
        ApprovedAtUtc = null;
        UpdatedAtUtc = nowUtc;
    }

    public void Hide(DateTimeOffset nowUtc)
    {
        if (Status != ListingStatus.Approved)
        {
            throw new InvalidOperationException(
                "Only approved listings can be hidden.");
        }

        Status = ListingStatus.Hidden;
        UpdatedAtUtc = nowUtc;
    }

    public void UpdateCoreDetails(
        int propertyTypeId,
        string title,
        string description,
        ListingPurpose purpose,
        decimal priceAmount,
        decimal areaSqm,
        string province,
        string district,
        string addressLine,
        DateTimeOffset nowUtc,
        int? bedrooms = null,
        int? bathrooms = null)
    {
        if (Status is ListingStatus.Pending or ListingStatus.Hidden)
        {
            throw new InvalidOperationException(
                "Listings in the current status cannot be edited.");
        }

        ValidateRequiredFields(
            OwnerId,
            propertyTypeId,
            title,
            province,
            district,
            addressLine);

        ValidatePriceAndArea(priceAmount, areaSqm);
        ValidateRooms(bedrooms, bathrooms);
        ValidatePurpose(purpose);
        ArgumentNullException.ThrowIfNull(description);

        PropertyTypeId = propertyTypeId;
        Title = title.Trim();
        Description = description.Trim();
        Purpose = purpose;
        PriceAmount = priceAmount;
        AreaSqm = areaSqm;
        Province = province.Trim();
        District = district.Trim();
        AddressLine = addressLine.Trim();
        Bedrooms = bedrooms;
        Bathrooms = bathrooms;

        if (Status == ListingStatus.Approved)
        {
            Status = ListingStatus.Pending;
            SubmittedAtUtc = nowUtc;
            ApprovedAtUtc = null;
        }
        else if (Status == ListingStatus.Rejected)
        {
            Status = ListingStatus.Draft;
            SubmittedAtUtc = null;
            ApprovedAtUtc = null;
            RejectionReason = null;
        }

        UpdatedAtUtc = nowUtc;
    }

    private static void ValidatePurpose(ListingPurpose purpose)
    {
        if (!Enum.IsDefined(purpose))
        {
            throw new ArgumentOutOfRangeException(nameof(purpose));
        }
    }

    private static void ValidateRequiredFields(
        string ownerId,
        int propertyTypeId,
        string title,
        string province,
        string district,
        string addressLine)
    {
        if (string.IsNullOrWhiteSpace(ownerId))
        {
            throw new ArgumentException(
                "Owner ID is required.",
                nameof(ownerId));
        }

        if (propertyTypeId <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(propertyTypeId));
        }

        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException(
                "Title is required.",
                nameof(title));
        }

        if (string.IsNullOrWhiteSpace(province))
        {
            throw new ArgumentException(
                "Province is required.",
                nameof(province));
        }

        if (string.IsNullOrWhiteSpace(district))
        {
            throw new ArgumentException(
                "District is required.",
                nameof(district));
        }

        if (string.IsNullOrWhiteSpace(addressLine))
        {
            throw new ArgumentException(
                "Address is required.",
                nameof(addressLine));
        }
    }

    private static void ValidatePriceAndArea(
        decimal priceAmount,
        decimal areaSqm)
    {
        if (priceAmount <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(priceAmount),
                "Price must be greater than zero.");
        }

        if (areaSqm <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(areaSqm),
                "Area must be greater than zero.");
        }
    }

    private static void ValidateRooms(int? bedrooms, int? bathrooms)
    {
        if (bedrooms < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(bedrooms),
                "Bedrooms cannot be negative.");
        }

        if (bathrooms < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(bathrooms),
                "Bathrooms cannot be negative.");
        }
    }
}
