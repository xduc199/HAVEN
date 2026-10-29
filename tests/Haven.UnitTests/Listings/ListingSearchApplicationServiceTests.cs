using Haven.Application.Abstractions;
using Haven.Application.Listings;
using Haven.Domain.Properties;
using Moq;

namespace Haven.UnitTests.Listings;

public sealed class ListingSearchApplicationServiceTests
{
    private readonly Mock<IListingRepository> _listingRepository = new();
    private readonly ListingApplicationService _service;

    public ListingSearchApplicationServiceTests()
    {
        _service = new ListingApplicationService(
            _listingRepository.Object,
            Mock.Of<IPropertyTypeRepository>(),
            TimeProvider.System);
    }

    [Fact]
    public async Task SearchPublicAsync_NormalizesOptionalTextAndUsesDefaultSort()
    {
        var expected = new ListingSearchResult([], 1, 20, 0, 0);
        ListingSearchCriteria? passedCriteria = null;
        _listingRepository
            .Setup(repository => repository.SearchPublicAsync(
                It.IsAny<ListingSearchCriteria>(),
                It.IsAny<CancellationToken>()))
            .Callback<ListingSearchCriteria, CancellationToken>((criteria, _) =>
                passedCriteria = criteria)
            .ReturnsAsync(expected);

        var result = await _service.SearchPublicAsync(new ListingSearchCriteria
        {
            Keyword = "  ",
            Province = " Da Nang ",
            District = " Son Tra ",
            SortBy = " PRICE_ASC "
        });

        Assert.Same(expected, result);
        Assert.NotNull(passedCriteria);
        Assert.Null(passedCriteria.Keyword);
        Assert.Equal("Da Nang", passedCriteria.Province);
        Assert.Equal("Son Tra", passedCriteria.District);
        Assert.Equal("price_asc", passedCriteria.SortBy);
    }

    [Theory]
    [MemberData(nameof(InvalidCriteria))]
    public async Task SearchPublicAsync_InvalidCriteriaDoesNotCallRepository(
        ListingSearchCriteria criteria)
    {
        await Assert.ThrowsAsync<ListingSearchValidationException>(
            () => _service.SearchPublicAsync(criteria));

        _listingRepository.Verify(
            repository => repository.SearchPublicAsync(
                It.IsAny<ListingSearchCriteria>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    public static IEnumerable<object[]> InvalidCriteria()
    {
        yield return [new ListingSearchCriteria { Page = 0 }];
        yield return [new ListingSearchCriteria { PageSize = -1 }];
        yield return [new ListingSearchCriteria { PageSize = 101 }];
        yield return [new ListingSearchCriteria { Page = int.MaxValue, PageSize = 100 }];
        yield return [new ListingSearchCriteria { PropertyTypeId = 0 }];
        yield return [new ListingSearchCriteria { Purpose = (ListingPurpose)99 }];
        yield return [new ListingSearchCriteria { MinPriceAmount = 10, MaxPriceAmount = 9 }];
        yield return [new ListingSearchCriteria { MinAreaSqm = -1 }];
        yield return [new ListingSearchCriteria { SortBy = "price desc; DROP TABLE Listings" }];
        yield return [new ListingSearchCriteria { Keyword = new string('x', 201) }];
    }
}
