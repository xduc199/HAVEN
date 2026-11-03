using Haven.Domain.Consultations;

namespace Haven.UnitTests.Consultations;

public sealed class ConsultationRequestTests
{
    private static readonly DateTimeOffset Now =
        new(2026, 11, 3, 10, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Create_TrimsContactFieldsAndStartsInNewStatus()
    {
        var request = ConsultationRequest.Create(
            " requester-id ",
            " Customer Name ",
            " customer@example.com ",
            " +1 555 234 5678 ",
            listingId: 5,
            message: " Please call me. ",
            Now);

        Assert.Equal("requester-id", request.RequesterId);
        Assert.Equal("Customer Name", request.ContactName);
        Assert.Equal("customer@example.com", request.Email);
        Assert.Equal("+1 555 234 5678", request.PhoneNumber);
        Assert.Equal("Please call me.", request.Message);
        Assert.Equal(ConsultationStatus.New, request.Status);
        Assert.Equal(Now, request.CreatedAtUtc);
        Assert.Equal(Now, request.UpdatedAtUtc);
    }

    [Theory]
    [InlineData("not-an-email", "+1 555 234 5678")]
    [InlineData("customer@example.com", "invalid phone")]
    public void Create_WithInvalidContactData_ThrowsArgumentException(
        string email,
        string phoneNumber)
    {
        Assert.Throws<ArgumentException>(() =>
            ConsultationRequest.Create(
                "requester-id",
                "Customer Name",
                email,
                phoneNumber,
                listingId: null,
                message: null,
                Now));
    }

    [Fact]
    public void ChangeStatus_RejectsUndefinedStatus()
    {
        var request = ConsultationRequest.Create(
            "requester-id",
            "Customer Name",
            "customer@example.com",
            "+1 555 234 5678",
            listingId: null,
            message: null,
            Now);

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            request.ChangeStatus((ConsultationStatus)99, Now.AddMinutes(1)));
        Assert.Equal(ConsultationStatus.New, request.Status);
        Assert.Equal(Now, request.UpdatedAtUtc);
    }
}
