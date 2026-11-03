using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Haven.Application.Abstractions;
using Haven.Domain.Properties;

namespace Haven.Application.Tags;

public sealed class TagApplicationService
{
    private static readonly Regex SlugSeparators = new("[^a-z0-9]+", RegexOptions.Compiled);
    private readonly ITagRepository _tagRepository;
    private readonly IListingRepository _listingRepository;
    private readonly TimeProvider _timeProvider;

    public TagApplicationService(
        ITagRepository tagRepository,
        IListingRepository listingRepository,
        TimeProvider timeProvider)
    {
        _tagRepository = tagRepository;
        _listingRepository = listingRepository;
        _timeProvider = timeProvider;
    }

    public async Task<TagPage> GetAdminPageAsync(
        string? search,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        ValidatePage(page, pageSize);
        return await _tagRepository.GetAdminPageAsync(
            NormalizeSearch(search),
            page,
            pageSize,
            cancellationToken);
    }

    public async Task<TagPage> GetPublicPageAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        ValidatePage(page, pageSize);
        return await _tagRepository.GetPublicPageAsync(page, pageSize, cancellationToken);
    }

    public Task<Tag?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        ValidateId(id);
        return _tagRepository.GetByIdAsync(id, cancellationToken);
    }

    public async Task<Tag?> GetBySlugAsync(
        string slug,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(slug);
        var tag = await _tagRepository.GetBySlugAsync(slug, cancellationToken);
        return tag is { IsActive: true } ? tag : null;
    }

    public async Task<Tag> CreateAsync(
        string name,
        string? description,
        CancellationToken cancellationToken = default)
    {
        var values = Normalize(name);
        await EnsureUniqueAsync(values.NormalizedName, values.Slug, null, cancellationToken);
        var tag = Tag.Create(
            values.Name,
            values.NormalizedName,
            values.Slug,
            description,
            _timeProvider.GetUtcNow());
        await _tagRepository.AddAsync(tag, cancellationToken);
        await _tagRepository.SaveChangesAsync(cancellationToken);
        return tag;
    }

    public async Task<Tag?> UpdateAsync(
        int id,
        string name,
        string? description,
        bool isActive,
        CancellationToken cancellationToken = default)
    {
        ValidateId(id);
        var tag = await _tagRepository.GetByIdAsync(id, cancellationToken);
        if (tag is null)
        {
            return null;
        }

        var values = Normalize(name);
        await EnsureUniqueAsync(values.NormalizedName, values.Slug, id, cancellationToken);
        var now = _timeProvider.GetUtcNow();
        tag.Update(values.Name, values.NormalizedName, values.Slug, description, now);
        if (isActive)
        {
            tag.Activate(now);
        }
        else
        {
            tag.Deactivate(now);
        }

        await _tagRepository.SaveChangesAsync(cancellationToken);
        return tag;
    }

    public async Task<IReadOnlyList<Tag>> GetForListingAsync(
        int listingId,
        CancellationToken cancellationToken = default)
    {
        ValidateId(listingId);
        return await _tagRepository.GetForListingAsync(listingId, cancellationToken);
    }

    public async Task<TagAssignmentResult> AssignToListingAsync(
        int listingId,
        int tagId,
        CancellationToken cancellationToken = default)
    {
        ValidateId(listingId);
        ValidateId(tagId);
        if (await _listingRepository.GetByIdAsync(listingId, cancellationToken) is null)
        {
            return TagAssignmentResult.ListingNotFound;
        }

        var tag = await _tagRepository.GetByIdAsync(tagId, cancellationToken);
        if (tag is null)
        {
            return TagAssignmentResult.TagNotFound;
        }

        if (!tag.IsActive)
        {
            return TagAssignmentResult.TagInactive;
        }

        return await _tagRepository.AssignToListingAsync(
            listingId,
            tagId,
            cancellationToken)
            ? TagAssignmentResult.Added
            : TagAssignmentResult.AlreadyAssigned;
    }

    public async Task<bool> RemoveFromListingAsync(
        int listingId,
        int tagId,
        CancellationToken cancellationToken = default)
    {
        ValidateId(listingId);
        ValidateId(tagId);
        return await _tagRepository.RemoveFromListingAsync(
            listingId,
            tagId,
            cancellationToken);
    }

    public async Task<TagListingPage> GetListingsAsync(
        int tagId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        ValidateId(tagId);
        ValidatePage(page, pageSize);
        return await _listingRepository.GetApprovedByTagAsync(
            tagId,
            page,
            pageSize,
            cancellationToken);
    }

    private async Task EnsureUniqueAsync(
        string normalizedName,
        string slug,
        int? excludingId,
        CancellationToken cancellationToken)
    {
        if (await _tagRepository.FindDuplicateAsync(
                normalizedName,
                slug,
                excludingId,
                cancellationToken) is not null)
        {
            throw new DuplicateTagException(slug);
        }
    }

    private static (string Name, string NormalizedName, string Slug) Normalize(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        var normalizedName = name.Trim().Normalize(NormalizationForm.FormKC);
        if (normalizedName.Length > Tag.NameMaxLength)
        {
            throw new ArgumentException($"Tag name cannot exceed {Tag.NameMaxLength} characters.", nameof(name));
        }

        var slugInput = normalizedName.Replace('đ', 'd').Replace('Đ', 'D')
            .Normalize(NormalizationForm.FormD);
        var slugBuilder = new StringBuilder(slugInput.Length);
        foreach (var character in slugInput)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark)
            {
                slugBuilder.Append(character);
            }
        }

        var slug = SlugSeparators.Replace(slugBuilder.ToString().ToLowerInvariant(), "-").Trim('-');
        if (string.IsNullOrWhiteSpace(slug))
        {
            throw new ArgumentException("Tag name must include a letter or number.", nameof(name));
        }

        var canonicalName = normalizedName.Normalize(NormalizationForm.FormC);
        return (canonicalName, canonicalName.ToUpperInvariant(), slug);
    }

    private static string? NormalizeSearch(string? search)
    {
        if (string.IsNullOrWhiteSpace(search))
        {
            return null;
        }

        var value = search.Trim();
        if (value.Length > Tag.NameMaxLength)
        {
            throw new ArgumentOutOfRangeException(
                nameof(search),
                $"Tag search cannot exceed {Tag.NameMaxLength} characters.");
        }

        return value;
    }

    private static void ValidatePage(int page, int pageSize)
    {
        if (page < 1 || pageSize is < 1 or > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(page), "Page or page size is invalid.");
        }
    }

    private static void ValidateId(int id)
    {
        if (id <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(id));
        }
    }
}
