using Haven.Application.Tags;
using Haven.Domain.Properties;

namespace Haven.Application.Abstractions;

public interface ITagRepository
{
    Task<Tag?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Tag?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<Tag?> FindDuplicateAsync(
        string normalizedName,
        string slug,
        int? excludingId,
        CancellationToken cancellationToken = default);
    Task<TagPage> GetAdminPageAsync(
        string? search,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
    Task<TagPage> GetPublicPageAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Tag>> GetForListingAsync(
        int listingId,
        CancellationToken cancellationToken = default);
    Task<bool> AssignToListingAsync(
        int listingId,
        int tagId,
        CancellationToken cancellationToken = default);
    Task<bool> RemoveFromListingAsync(
        int listingId,
        int tagId,
        CancellationToken cancellationToken = default);
    Task AddAsync(Tag tag, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
