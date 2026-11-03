using Haven.Application.Abstractions;
using Haven.Application.Tags;
using Haven.Domain.Properties;
using Haven.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Haven.Infrastructure.Repositories;

public sealed class EfTagRepository(ApplicationDbContext dbContext) : ITagRepository
{
    public Task<Tag?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default) =>
        dbContext.Tags.FirstOrDefaultAsync(tag => tag.Id == id, cancellationToken);

    public Task<Tag?> GetBySlugAsync(
        string slug,
        CancellationToken cancellationToken = default) =>
        dbContext.Tags.AsNoTracking()
            .FirstOrDefaultAsync(tag =>
                tag.Slug == slug &&
                tag.IsActive &&
                tag.ListingTags.Any(link =>
                    link.Listing.Status == ListingStatus.Approved),
                cancellationToken);

    public Task<Tag?> FindDuplicateAsync(
        string normalizedName,
        string slug,
        int? excludingId,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.Tags.AsNoTracking();
        if (excludingId.HasValue)
        {
            query = query.Where(tag => tag.Id != excludingId.Value);
        }

        return query.FirstOrDefaultAsync(
            tag => tag.NormalizedName == normalizedName || tag.Slug == slug,
            cancellationToken);
    }

    public async Task<TagPage> GetAdminPageAsync(
        string? search,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Tag> query = dbContext.Tags.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(tag =>
                tag.Name.Contains(search) || tag.Slug.Contains(search));
        }

        var totalCount = await query.LongCountAsync(cancellationToken);
        var items = await query.OrderBy(tag => tag.Name)
            .ThenBy(tag => tag.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
        return CreatePage(items, page, pageSize, totalCount);
    }

    public async Task<TagPage> GetPublicPageAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.Tags.AsNoTracking().Where(tag =>
            tag.IsActive && tag.ListingTags.Any(link =>
                link.Listing.Status == ListingStatus.Approved));
        var totalCount = await query.LongCountAsync(cancellationToken);
        var items = await query.OrderBy(tag => tag.Name)
            .ThenBy(tag => tag.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
        return CreatePage(items, page, pageSize, totalCount);
    }

    public async Task<IReadOnlyList<Tag>> GetForListingAsync(
        int listingId,
        CancellationToken cancellationToken = default) =>
        await dbContext.ListingTags.AsNoTracking()
            .Where(link => link.ListingId == listingId)
            .Select(link => link.Tag)
            .OrderBy(tag => tag.Name)
            .ToListAsync(cancellationToken);

    public async Task<bool> AssignToListingAsync(
        int listingId,
        int tagId,
        CancellationToken cancellationToken = default)
    {
        if (await dbContext.ListingTags.AnyAsync(
                link => link.ListingId == listingId && link.TagId == tagId,
                cancellationToken))
        {
            return false;
        }

        dbContext.ListingTags.Add(ListingTag.Create(listingId, tagId));
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> RemoveFromListingAsync(
        int listingId,
        int tagId,
        CancellationToken cancellationToken = default)
    {
        var link = await dbContext.ListingTags.FirstOrDefaultAsync(
            candidate => candidate.ListingId == listingId && candidate.TagId == tagId,
            cancellationToken);
        if (link is null)
        {
            return false;
        }

        dbContext.ListingTags.Remove(link);
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public Task AddAsync(Tag tag, CancellationToken cancellationToken = default) =>
        dbContext.Tags.AddAsync(tag, cancellationToken).AsTask();

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        dbContext.SaveChangesAsync(cancellationToken);

    private static TagPage CreatePage(
        IReadOnlyList<Tag> items,
        int page,
        int pageSize,
        long totalCount) =>
        new(
            items,
            page,
            pageSize,
            totalCount,
            totalCount == 0 ? 0 : (totalCount + pageSize - 1) / pageSize);
}
