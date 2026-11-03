using Haven.Application.Abstractions;
using Haven.Application.Consultations;
using Haven.Domain.Consultations;
using Moq;

namespace Haven.UnitTests.Consultations;

public sealed class AdminConsultationSearchTests
{
    [Fact]
    public async Task SearchForAdminAsync_PassesSearchDateStatusAndPaginationToRepository()
    {
        var repository = new Mock<IConsultationRepository>();
        var from = new DateTimeOffset(2026, 10, 1, 0, 0, 0, TimeSpan.Zero);
        var to = from.AddDays(8);
        var criteria = new AdminConsultationCriteria(
            "  person@example.com ",
            ConsultationStatus.InProgress,
            from,
            to,
            2,
            10);
        repository.Setup(value => value.SearchForAdminAsync(
                It.Is<AdminConsultationCriteria>(actual =>
                    actual.Search == "person@example.com" &&
                    actual.Status == criteria.Status &&
                    actual.FromUtc == from &&
                    actual.ToUtcExclusive == to &&
                    actual.Page == 2 &&
                    actual.PageSize == 10),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ConsultationPage([], 11));
        var service = new ConsultationApplicationService(
            repository.Object,
            new Mock<IListingRepository>().Object,
            TimeProvider.System);

        var result = await service.SearchForAdminAsync(criteria);

        Assert.Equal(2, result.Page);
        Assert.Equal(2, result.TotalPages);
        Assert.Equal(11, result.TotalCount);
    }
}
