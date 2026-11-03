using System.ComponentModel.DataAnnotations;
using Haven.Domain.Properties;

namespace Haven.Web.Models.Tags;

public sealed class CreateTagRequest
{
    [Required]
    [StringLength(Tag.NameMaxLength, MinimumLength = 1)]
    public string Name { get; set; } = string.Empty;

    [StringLength(Tag.DescriptionMaxLength)]
    public string? Description { get; set; }
}

public sealed class UpdateTagRequest
{
    [Required]
    [StringLength(Tag.NameMaxLength, MinimumLength = 1)]
    public string Name { get; set; } = string.Empty;

    [StringLength(Tag.DescriptionMaxLength)]
    public string? Description { get; set; }

    public bool IsActive { get; set; }
}

public sealed record TagResponse(
    int Id,
    string Name,
    string Slug,
    string? Description,
    bool IsActive,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc)
{
    public static TagResponse From(Tag tag) => new(
        tag.Id,
        tag.Name,
        tag.Slug,
        tag.Description,
        tag.IsActive,
        tag.CreatedAtUtc,
        tag.UpdatedAtUtc);
}

public sealed record TagPageResponse(
    IReadOnlyList<TagResponse> Items,
    int Page,
    int PageSize,
    long TotalCount,
    long TotalPages);

public sealed record PublicTagListingResponse(
    int Id,
    int PropertyTypeId,
    string Title,
    ListingPurpose Purpose,
    decimal PriceAmount,
    decimal AreaSqm,
    string Province,
    string District,
    int? Bedrooms,
    DateTimeOffset CreatedAtUtc);

public sealed record PublicTagListingPageResponse(
    IReadOnlyList<PublicTagListingResponse> Items,
    int Page,
    int PageSize,
    long TotalCount,
    long TotalPages);
