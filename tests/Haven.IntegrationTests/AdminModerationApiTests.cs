using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
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

public sealed class AdminModerationApiTests
{
    [Fact]
    public async Task Admin_CanReviewPendingListingsAndHideApprovedListings()
    {
        using var factory = CreateFactory();
        var (approvedId, pendingIds) = await SeedListingsAsync(factory);
        using var client = CreateClient(factory);
        await SignInAdminAsync(client);

        using var queueResponse = await client.GetAsync(
            "/api/admin/moderation/pending?page=1&pageSize=10");
        Assert.Equal(HttpStatusCode.OK, queueResponse.StatusCode);

        using var queueBody = JsonDocument.Parse(
            await queueResponse.Content.ReadAsStringAsync());
        var items = queueBody.RootElement.GetProperty("items");
        Assert.Equal(2, items.GetArrayLength());
        Assert.Equal(2, queueBody.RootElement.GetProperty("totalCount").GetInt64());
        Assert.All(
            items.EnumerateArray(),
            item => Assert.Equal(
                (int)ListingStatus.Pending,
                item.GetProperty("status").GetInt32()));

        using var approveResponse = await client.PostAsync(
            $"/api/admin/moderation/{pendingIds[0]}/approve",
            content: null);
        Assert.Equal(HttpStatusCode.OK, approveResponse.StatusCode);
        using var approvedBody = JsonDocument.Parse(
            await approveResponse.Content.ReadAsStringAsync());
        Assert.Equal(
            (int)ListingStatus.Approved,
            approvedBody.RootElement.GetProperty("status").GetInt32());

        using var publicApprovedResponse = await client.GetAsync(
            $"/api/listings/{pendingIds[0]}");
        Assert.Equal(HttpStatusCode.OK, publicApprovedResponse.StatusCode);

        using var rejectResponse = await client.PostAsJsonAsync(
            $"/api/admin/moderation/{pendingIds[1]}/reject",
            new { reason = "Incomplete property information." });
        Assert.Equal(HttpStatusCode.OK, rejectResponse.StatusCode);
        using var rejectedBody = JsonDocument.Parse(
            await rejectResponse.Content.ReadAsStringAsync());
        Assert.Equal(
            (int)ListingStatus.Rejected,
            rejectedBody.RootElement.GetProperty("status").GetInt32());
        Assert.Equal(
            "Incomplete property information.",
            rejectedBody.RootElement.GetProperty("rejectionReason").GetString());

        using var publicRejectedResponse = await client.GetAsync(
            $"/api/listings/{pendingIds[1]}");
        Assert.Equal(HttpStatusCode.NotFound, publicRejectedResponse.StatusCode);

        using var hideResponse = await client.PostAsync(
            $"/api/admin/moderation/{approvedId}/hide",
            content: null);
        Assert.Equal(HttpStatusCode.OK, hideResponse.StatusCode);

        using var publicHiddenResponse = await client.GetAsync(
            $"/api/listings/{approvedId}");
        Assert.Equal(HttpStatusCode.NotFound, publicHiddenResponse.StatusCode);
    }

    [Fact]
    public async Task Moderation_RequiresAdminRole()
    {
        using var factory = CreateFactory();
        using var anonymousClient = CreateClient(factory);

        using var anonymousResponse = await anonymousClient.GetAsync(
            "/api/admin/moderation/pending");
        Assert.Equal(HttpStatusCode.Unauthorized, anonymousResponse.StatusCode);

        using var userClient = CreateClient(factory);
        using var registrationResponse = await userClient.PostAsJsonAsync(
            "/api/auth/register",
            new
            {
                displayName = "Regular User",
                email = "regular.user@example.com",
                password = "StrongPassword123!"
            });
        Assert.Equal(HttpStatusCode.Created, registrationResponse.StatusCode);

        using var forbiddenResponse = await userClient.GetAsync(
            "/api/admin/moderation/pending");
        Assert.Equal(HttpStatusCode.Forbidden, forbiddenResponse.StatusCode);
    }

    [Fact]
    public async Task Reject_WithoutReason_ReturnsValidationProblem()
    {
        using var factory = CreateFactory();
        var (_, pendingIds) = await SeedListingsAsync(factory);
        using var client = CreateClient(factory);
        await SignInAdminAsync(client);

        using var response = await client.PostAsJsonAsync(
            $"/api/admin/moderation/{pendingIds[0]}/reject",
            new { reason = " " });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private static async Task<(int ApprovedId, int[] PendingIds)> SeedListingsAsync(
        WebApplicationFactory<Program> factory)
    {
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider
            .GetRequiredService<ApplicationDbContext>();
        dbContext.PropertyTypes.Add(PropertyType.Create("House"));

        var firstPending = CreateListing("Pending one");
        firstPending.SubmitForReview(DateTimeOffset.UtcNow);
        var secondPending = CreateListing("Pending two");
        secondPending.SubmitForReview(DateTimeOffset.UtcNow);
        var approved = CreateListing("Approved");
        approved.SubmitForReview(DateTimeOffset.UtcNow);
        approved.Approve(DateTimeOffset.UtcNow);

        dbContext.Listings.AddRange(firstPending, secondPending, approved);
        await dbContext.SaveChangesAsync();
        return (approved.Id, [firstPending.Id, secondPending.Id]);
    }

    private static Listing CreateListing(string title) =>
        Listing.Create(
            "moderation-owner",
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
