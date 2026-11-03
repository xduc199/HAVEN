using Haven.Application.Abstractions;
using Haven.Application.Tags;
using Haven.Domain.Properties;
using Moq;

namespace Haven.UnitTests.Tags;

public sealed class TagApplicationServiceTests
{
    private static readonly DateTimeOffset Now =
        new(2026, 10, 26, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task CreateAsync_NormalizesVietnameseNameAndPersistsTag()
    {
        var tags = new Mock<ITagRepository>();
        var listings = new Mock<IListingRepository>();
        tags.Setup(repository => repository.FindDuplicateAsync(
                "NHÀ ĐẸP HÀ NỘI",
                "nha-dep-ha-noi",
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Tag?)null);
        Tag? created = null;
        tags.Setup(repository => repository.AddAsync(
                It.IsAny<Tag>(),
                It.IsAny<CancellationToken>()))
            .Callback<Tag, CancellationToken>((tag, _) => created = tag)
            .Returns(Task.CompletedTask);
        tags.Setup(repository => repository.SaveChangesAsync(
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var service = new TagApplicationService(
            tags.Object,
            listings.Object,
            new FixedTimeProvider(Now));

        var result = await service.CreateAsync(" Nhà đẹp Hà Nội ", " Mô tả ");

        Assert.Equal("Nhà đẹp Hà Nội", result.Name);
        Assert.Equal("nha-dep-ha-noi", result.Slug);
        Assert.Equal("Mô tả", result.Description);
        Assert.Same(result, created);
        tags.Verify(repository => repository.SaveChangesAsync(
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WhenNormalizedNameAlreadyExists_RejectsWithoutSaving()
    {
        var tags = new Mock<ITagRepository>();
        var existing = Tag.Create(
            "Nhà đẹp Hà Nội",
            "NHÀ ĐẸP HÀ NỘI",
            "nha-dep-ha-noi",
            null,
            Now);
        tags.Setup(repository => repository.FindDuplicateAsync(
                "NHÀ ĐẸP HÀ NỘI",
                "nha-dep-ha-noi",
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        var service = new TagApplicationService(
            tags.Object,
            new Mock<IListingRepository>().Object,
            new FixedTimeProvider(Now));

        await Assert.ThrowsAsync<DuplicateTagException>(() =>
            service.CreateAsync("NHÀ ĐẸP HÀ NỘI", null));
        tags.Verify(repository => repository.AddAsync(
            It.IsAny<Tag>(), It.IsAny<CancellationToken>()), Times.Never);
        tags.Verify(repository => repository.SaveChangesAsync(
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task AssignToListingAsync_WhenListingAndTagAreActive_AssignsTag()
    {
        var tags = new Mock<ITagRepository>();
        var listings = new Mock<IListingRepository>();
        listings.Setup(repository => repository.GetByIdAsync(
                7,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Listing.Create(
                "owner",
                1,
                "Title",
                "Description",
                ListingPurpose.Sale,
                100_000,
                50,
                "Province",
                "District",
                "Address",
                Now));
        tags.Setup(repository => repository.GetByIdAsync(
                3,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Tag.Create("House", "HOUSE", "house", null, Now));
        tags.Setup(repository => repository.AssignToListingAsync(
                7,
                3,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var service = new TagApplicationService(
            tags.Object,
            listings.Object,
            new FixedTimeProvider(Now));

        var result = await service.AssignToListingAsync(7, 3);

        Assert.Equal(TagAssignmentResult.Added, result);
    }

    [Fact]
    public async Task AssignToListingAsync_WhenTagIsInactiveDoesNotPersistAssignment()
    {
        var tags = new Mock<ITagRepository>();
        var listings = new Mock<IListingRepository>();
        listings.Setup(repository => repository.GetByIdAsync(
                7,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Listing.Create(
                "owner",
                1,
                "Title",
                "Description",
                ListingPurpose.Sale,
                100_000,
                50,
                "Province",
                "District",
                "Address",
                Now));
        var inactiveTag = Tag.Create("House", "HOUSE", "house", null, Now);
        inactiveTag.Deactivate(Now.AddMinutes(1));
        tags.Setup(repository => repository.GetByIdAsync(
                3,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(inactiveTag);

        var service = new TagApplicationService(
            tags.Object,
            listings.Object,
            new FixedTimeProvider(Now));

        var result = await service.AssignToListingAsync(7, 3);

        Assert.Equal(TagAssignmentResult.TagInactive, result);
        tags.Verify(repository => repository.AssignToListingAsync(
            It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RemoveFromListingAsync_ReturnsWhetherAssignmentExisted()
    {
        var tags = new Mock<ITagRepository>();
        tags.Setup(repository => repository.RemoveFromListingAsync(
                7,
                3,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        var service = new TagApplicationService(
            tags.Object,
            new Mock<IListingRepository>().Object,
            new FixedTimeProvider(Now));

        var removed = await service.RemoveFromListingAsync(7, 3);

        Assert.True(removed);
        tags.Verify(repository => repository.RemoveFromListingAsync(
            7, 3, It.IsAny<CancellationToken>()), Times.Once);
    }

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}
