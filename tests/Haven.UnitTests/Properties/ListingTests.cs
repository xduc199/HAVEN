using Haven.Domain.Properties;

namespace Haven.UnitTests.Properties;

public sealed class ListingTests
{
    private static readonly DateTimeOffset Now =
        new(2026, 10, 26, 13, 0, 0, TimeSpan.Zero);

    private static Listing CreateListing()
    {
        return Listing.Create(
            ownerId: "user-123",
            propertyTypeId: 1,
            title: "  Nhà phố trung tâm  ",
            description: "Mô tả căn nhà",
            purpose: ListingPurpose.Sale,
            priceAmount: 2_500_000_000m,
            areaSqm: 80m,
            province: "Thanh Hóa",
            district: "Đông Sơn",
            addressLine: "12 Đường Ví Dụ",
            nowUtc: Now);
    }

    [Fact]
    public void Create_WithValidData_ShouldCreateDraftListing()
    {
        var listing = CreateListing();

        Assert.Equal(ListingStatus.Draft, listing.Status);
        Assert.Equal("user-123", listing.OwnerId);
        Assert.Equal("Nhà phố trung tâm", listing.Title);
        Assert.Equal(2_500_000_000m, listing.PriceAmount);
        Assert.Equal(80m, listing.AreaSqm);
        Assert.Equal(Now, listing.CreatedAtUtc);
        Assert.Equal(Now, listing.UpdatedAtUtc);
        Assert.Null(listing.SubmittedAtUtc);
        Assert.Null(listing.ApprovedAtUtc);
    }

    [Fact]
    public void Create_WithZeroPrice_ShouldThrow()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            Listing.Create(
                "user-123", 1, "Nhà phố", "Mô tả",
                ListingPurpose.Sale, 0m, 80m,
                "Thanh Hóa", "Đông Sơn", "Địa chỉ",
                Now));
    }

    [Fact]
    public void Create_WithZeroArea_ShouldThrow()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            Listing.Create(
                "user-123", 1, "Nhà phố", "Mô tả",
                ListingPurpose.Sale, 1_000_000_000m, 0m,
                "Thanh Hóa", "Đông Sơn", "Địa chỉ",
                Now));
    }

    [Fact]
    public void Create_WithBlankTitle_ShouldThrow()
    {
        Assert.Throws<ArgumentException>(() =>
            Listing.Create(
                "user-123", 1, "   ", "Mô tả",
                ListingPurpose.Sale, 1_000_000_000m, 80m,
                "Thanh Hóa", "Đông Sơn", "Địa chỉ",
                Now));
    }

    [Fact]
    public void SubmitForReview_FromDraft_ShouldBecomePending()
    {
        var listing = CreateListing();
        var submittedAt = Now.AddMinutes(5);

        listing.SubmitForReview(submittedAt);

        Assert.Equal(ListingStatus.Pending, listing.Status);
        Assert.Equal(submittedAt, listing.SubmittedAtUtc);
        Assert.Equal(submittedAt, listing.UpdatedAtUtc);
    }

    [Fact]
    public void SubmitForReview_FromPending_ShouldThrow()
    {
        var listing = CreateListing();
        listing.SubmitForReview(Now);

        Assert.Throws<InvalidOperationException>(() =>
            listing.SubmitForReview(Now.AddMinutes(1)));
    }

    [Fact]
    public void Approve_FromPending_ShouldBecomeApproved()
    {
        var listing = CreateListing();
        listing.SubmitForReview(Now);

        var approvedAt = Now.AddHours(1);
        listing.Approve(approvedAt);

        Assert.Equal(ListingStatus.Approved, listing.Status);
        Assert.Equal(approvedAt, listing.ApprovedAtUtc);
        Assert.Equal(approvedAt, listing.UpdatedAtUtc);
    }

    [Fact]
    public void Approve_FromDraft_ShouldThrow()
    {
        var listing = CreateListing();

        Assert.Throws<InvalidOperationException>(() =>
            listing.Approve(Now));
    }

    [Fact]
    public void Reject_WithBlankReason_ShouldThrow()
    {
        var listing = CreateListing();
        listing.SubmitForReview(Now);

        Assert.Throws<ArgumentException>(() =>
            listing.Reject("   ", Now.AddMinutes(1)));
    }

    [Fact]
    public void Reject_WithReason_ShouldStoreReasonAndBecomeRejected()
    {
        var listing = CreateListing();
        listing.SubmitForReview(Now);

        listing.Reject("  Thiếu thông tin pháp lý  ", Now.AddHours(1));

        Assert.Equal(ListingStatus.Rejected, listing.Status);
        Assert.Equal("Thiếu thông tin pháp lý", listing.RejectionReason);
        Assert.Null(listing.ApprovedAtUtc);
    }

    [Fact]
    public void UpdateCoreDetails_AfterApproval_ShouldReturnToPending()
    {
        var listing = CreateListing();
        listing.SubmitForReview(Now);
        listing.Approve(Now.AddMinutes(10));

        var updatedAt = Now.AddHours(2);

        listing.UpdateCoreDetails(
            propertyTypeId: 1,
            title: "Nhà phố đã cập nhật",
            description: "Mô tả mới",
            purpose: ListingPurpose.Sale,
            priceAmount: 2_700_000_000m,
            areaSqm: 85m,
            province: "Thanh Hóa",
            district: "Đông Sơn",
            addressLine: "12 Đường Ví Dụ",
            nowUtc: updatedAt);

        Assert.Equal(ListingStatus.Pending, listing.Status);
        Assert.Equal("Nhà phố đã cập nhật", listing.Title);
        Assert.Equal(2_700_000_000m, listing.PriceAmount);
        Assert.Equal(85m, listing.AreaSqm);
        Assert.Null(listing.ApprovedAtUtc);
        Assert.Equal(updatedAt, listing.SubmittedAtUtc);
    }

    [Fact]
    public void UpdateCoreDetails_WhilePending_ShouldThrow()
    {
        var listing = CreateListing();
        listing.SubmitForReview(Now);

        Assert.Throws<InvalidOperationException>(() =>
            listing.UpdateCoreDetails(
                1, "Tiêu đề mới", "Mô tả mới",
                ListingPurpose.Sale, 2_000_000_000m, 80m,
                "Thanh Hóa", "Đông Sơn", "Địa chỉ",
                Now.AddMinutes(1)));
    }

    [Fact]
    public void UpdateCoreDetails_AfterRejection_ShouldReturnToDraft()
    {
        var listing = CreateListing();
        listing.SubmitForReview(Now);
        listing.Reject("Thiếu giấy tờ", Now.AddMinutes(1));

        listing.UpdateCoreDetails(
            1, "Nhà đã bổ sung thông tin", "Mô tả mới",
            ListingPurpose.Sale, 2_000_000_000m, 80m,
            "Thanh Hóa", "Đông Sơn", "Địa chỉ",
            Now.AddMinutes(2));

        Assert.Equal(ListingStatus.Draft, listing.Status);
        Assert.Null(listing.RejectionReason);
        Assert.Null(listing.SubmittedAtUtc);
    }
}
