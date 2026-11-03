using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Haven.Domain.Consultations;
using Haven.Domain.Properties;
using Haven.Infrastructure.Context;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Haven.IntegrationTests;

public sealed class ConsultationApiTests
{
    [Fact]
    public async Task RequesterAndAdmin_CanViewAndManageConsultation()
    {
        using var factory = CreateFactory();
        var (approvedListingId, pendingListingId) =
            await SeedListingsAsync(factory);
        using var requester = CreateClient(factory);
        var requesterId = await RegisterAsync(
            requester,
            "requester@example.com",
            "Requester");

        using var createResponse = await requester.PostAsJsonAsync(
            "/api/consultations",
            CreateRequest(approvedListingId));
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        using var createdBody = JsonDocument.Parse(
            await createResponse.Content.ReadAsStringAsync());
        var requestId = createdBody.RootElement.GetProperty("id").GetInt32();
        Assert.Equal(
            (int)ConsultationStatus.New,
            createdBody.RootElement.GetProperty("status").GetInt32());
        Assert.False(createdBody.RootElement.TryGetProperty("requesterId", out _));
        Assert.Equal(
            requesterId,
            await GetStoredRequesterIdAsync(factory, requestId));

        using var ownListResponse = await requester.GetAsync(
            "/api/consultations?page=1&pageSize=10");
        Assert.Equal(HttpStatusCode.OK, ownListResponse.StatusCode);
        using var ownListBody = JsonDocument.Parse(
            await ownListResponse.Content.ReadAsStringAsync());
        Assert.Single(ownListBody.RootElement.GetProperty("items").EnumerateArray());

        using var otherUser = CreateClient(factory);
        await RegisterAsync(
            otherUser,
            "other.requester@example.com",
            "Other Requester");
        using var otherUserDetail = await otherUser.GetAsync(
            $"/api/consultations/{requestId}");
        using var otherUserList = await otherUser.GetAsync("/api/consultations");
        Assert.Equal(HttpStatusCode.NotFound, otherUserDetail.StatusCode);
        using var otherListBody = JsonDocument.Parse(
            await otherUserList.Content.ReadAsStringAsync());
        Assert.Empty(otherListBody.RootElement.GetProperty("items").EnumerateArray());

        using var admin = CreateClient(factory);
        await SignInAdminAsync(admin);
        using var adminListResponse = await admin.GetAsync(
            "/api/admin/consultations?status=New");
        Assert.Equal(HttpStatusCode.OK, adminListResponse.StatusCode);
        using var adminListBody = JsonDocument.Parse(
            await adminListResponse.Content.ReadAsStringAsync());
        Assert.Single(adminListBody.RootElement.GetProperty("items").EnumerateArray());
        Assert.Equal(
            "customer@example.com",
            adminListBody.RootElement
                .GetProperty("items")[0]
                .GetProperty("email")
                .GetString());
        using var adminDetailResponse = await admin.GetAsync(
            $"/api/admin/consultations/{requestId}");
        Assert.Equal(HttpStatusCode.OK, adminDetailResponse.StatusCode);

        using var updateStatusResponse = await admin.PutAsJsonAsync(
            $"/api/admin/consultations/{requestId}/status",
            new { status = ConsultationStatus.InProgress });
        Assert.Equal(HttpStatusCode.OK, updateStatusResponse.StatusCode);
        using var updatedBody = JsonDocument.Parse(
            await updateStatusResponse.Content.ReadAsStringAsync());
        Assert.Equal(
            (int)ConsultationStatus.InProgress,
            updatedBody.RootElement.GetProperty("status").GetInt32());

        using var adminListingResponse = await admin.GetAsync(
            "/api/admin/listings?page=1&pageSize=10");
        Assert.Equal(HttpStatusCode.OK, adminListingResponse.StatusCode);
        using var adminListingBody = JsonDocument.Parse(
            await adminListingResponse.Content.ReadAsStringAsync());
        Assert.Equal(2, adminListingBody.RootElement.GetProperty("totalCount").GetInt64());

        using var pendingResponse = await admin.GetAsync(
            $"/api/admin/listings/{pendingListingId}");
        Assert.Equal(HttpStatusCode.OK, pendingResponse.StatusCode);
        using var approvedResponse = await admin.GetAsync(
            $"/api/admin/listings/{approvedListingId}");
        Assert.Equal(HttpStatusCode.OK, approvedResponse.StatusCode);

        using var dashboardResponse = await admin.GetAsync(
            "/api/admin/dashboard");
        Assert.Equal(HttpStatusCode.OK, dashboardResponse.StatusCode);
        using var dashboardBody = JsonDocument.Parse(
            await dashboardResponse.Content.ReadAsStringAsync());
        Assert.Equal(
            2,
            dashboardBody.RootElement.GetProperty("totalListings").GetInt64());
        Assert.Equal(
            1,
            dashboardBody.RootElement.GetProperty("pendingListings").GetInt64());
        Assert.Equal(
            1,
            dashboardBody.RootElement.GetProperty("approvedListings").GetInt64());
        Assert.Equal(
            1,
            dashboardBody.RootElement.GetProperty("inProgressConsultations").GetInt64());
        Assert.Equal(
            0,
            dashboardBody.RootElement.GetProperty("newConsultations").GetInt64());
    }

    [Fact]
    public async Task Consultation_RequiresAuthenticationAndValidContactDetails()
    {
        using var factory = CreateFactory();
        using var anonymous = CreateClient(factory);

        using var anonymousCreate = await anonymous.PostAsJsonAsync(
            "/api/consultations",
            CreateRequest());
        using var anonymousList = await anonymous.GetAsync("/api/consultations");
        Assert.Equal(HttpStatusCode.Unauthorized, anonymousCreate.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, anonymousList.StatusCode);

        using var requester = CreateClient(factory);
        await RegisterAsync(
            requester,
            "validation.requester@example.com",
            "Validation Requester");

        using var invalidPhone = await requester.PostAsJsonAsync(
            "/api/consultations",
            new
            {
                contactName = "Customer",
                email = "customer@example.com",
                phoneNumber = "not-a-phone"
            });
        using var nonPublicListing = await requester.PostAsJsonAsync(
            "/api/consultations",
            CreateRequest(listingId: 999));

        Assert.Equal(HttpStatusCode.BadRequest, invalidPhone.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, nonPublicListing.StatusCode);
    }

    [Fact]
    public async Task ConsultationManagement_IsAdminOnly()
    {
        using var factory = CreateFactory();
        using var anonymous = CreateClient(factory);
        using var anonymousAdminResponse = await anonymous.GetAsync(
            "/api/admin/consultations");
        using var anonymousDashboardResponse = await anonymous.GetAsync(
            "/api/admin/dashboard");
        Assert.Equal(
            HttpStatusCode.Unauthorized,
            anonymousAdminResponse.StatusCode);
        Assert.Equal(
            HttpStatusCode.Unauthorized,
            anonymousDashboardResponse.StatusCode);

        using var user = CreateClient(factory);
        await RegisterAsync(user, "regular@example.com", "Regular User");
        using var forbiddenResponse = await user.GetAsync(
            "/api/admin/consultations");
        using var forbiddenDashboardResponse = await user.GetAsync(
            "/api/admin/dashboard");
        Assert.Equal(HttpStatusCode.Forbidden, forbiddenResponse.StatusCode);
        Assert.Equal(
            HttpStatusCode.Forbidden,
            forbiddenDashboardResponse.StatusCode);
    }

    private static object CreateRequest(int? listingId = null) =>
        new
        {
            contactName = "Customer Name",
            email = "customer@example.com",
            phoneNumber = "+1 (555) 234-5678",
            listingId,
            message = "Please contact me about this property."
        };

    private static async Task<(int ApprovedId, int PendingId)> SeedListingsAsync(
        WebApplicationFactory<Program> factory)
    {
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider
            .GetRequiredService<ApplicationDbContext>();
        dbContext.PropertyTypes.Add(PropertyType.Create("House"));

        var approved = CreateListing("Approved");
        approved.SubmitForReview(DateTimeOffset.UtcNow);
        approved.Approve(DateTimeOffset.UtcNow);
        var pending = CreateListing("Pending");
        pending.SubmitForReview(DateTimeOffset.UtcNow);
        dbContext.Listings.AddRange(approved, pending);
        await dbContext.SaveChangesAsync();
        return (approved.Id, pending.Id);
    }

    private static Listing CreateListing(string title) =>
        Listing.Create(
            "consultation-test-owner",
            1,
            title,
            "Description",
            ListingPurpose.Sale,
            1_000_000m,
            50m,
            "Province",
            "District",
            "Address",
            DateTimeOffset.UtcNow);

    private static async Task<string> RegisterAsync(
        HttpClient client,
        string email,
        string displayName)
    {
        using var response = await client.PostAsJsonAsync(
            "/api/auth/register",
            new
            {
                displayName,
                email,
                password = "StrongPassword123!"
            });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        using var body = JsonDocument.Parse(
            await response.Content.ReadAsStringAsync());
        return body.RootElement.GetProperty("id").GetString()!;
    }

    private static async Task<string?> GetStoredRequesterIdAsync(
        WebApplicationFactory<Program> factory,
        int requestId)
    {
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider
            .GetRequiredService<ApplicationDbContext>();
        return await dbContext.ConsultationRequests
            .Where(request => request.Id == requestId)
            .Select(request => request.RequesterId)
            .SingleOrDefaultAsync();
    }

    private static async Task SignInAdminAsync(HttpClient client)
    {
        using var response = await client.PostAsJsonAsync(
            "/api/auth/login",
            new
            {
                email = "admin@example.com",
                password = "AdminPassword123!"
            });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private static WebApplicationFactory<Program> CreateFactory()
    {
        var databaseName = Guid.NewGuid().ToString();
        return new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((_, configuration) =>
                configuration.AddInMemoryCollection(
                    new Dictionary<string, string?>
                    {
                        ["Haven:AdminBootstrap:Email"] = "admin@example.com",
                        ["Haven:AdminBootstrap:Password"] =
                            "AdminPassword123!"
                    }));
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<
                    IDbContextOptionsConfiguration<ApplicationDbContext>>();
                services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
                services.RemoveAll<ApplicationDbContext>();
                services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseInMemoryDatabase(databaseName));
            });
        });
    }

    private static HttpClient CreateClient(
        WebApplicationFactory<Program> factory) =>
        AntiforgeryTestClient.Create(factory, new WebApplicationFactoryClientOptions
        {
            HandleCookies = true,
            AllowAutoRedirect = false
        });
}
