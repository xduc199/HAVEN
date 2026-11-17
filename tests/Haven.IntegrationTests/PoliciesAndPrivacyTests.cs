using System.Net;
using System.Net.Http.Json;
using Haven.Domain.Privacy;
using Haven.Infrastructure.Context;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Configuration;
using System.Text.RegularExpressions;

namespace Haven.IntegrationTests;

public sealed class PoliciesAndPrivacyTests
{
    [Fact]
    public async Task PublicPolicyPagesAndFooterLinks_AreAvailableInVietnamese()
    {
        using var factory = CreateFactory();
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        foreach (var route in new[] { "/Privacy", "/Terms", "/PostingRules", "/Complaints", "/CookiePolicy", "/About" })
        {
            using var response = await client.GetAsync(route);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains("HAVEN", await response.Content.ReadAsStringAsync());
        }
        using var historical = await client.GetAsync("/Privacy?version=1.0.0");
        using var unknown = await client.GetAsync("/Privacy?version=0.0.1");
        using var home = await client.GetAsync("/");
        using var register = await client.GetAsync("/account/register");
        var markup = await home.Content.ReadAsStringAsync();
        var registerMarkup = await register.Content.ReadAsStringAsync();
        Assert.Equal(HttpStatusCode.OK, historical.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, unknown.StatusCode);
        Assert.Contains("Điều khoản sử dụng", markup);
        Assert.Contains("Chính sách cookie", markup);
        Assert.Contains("name=\"AcceptedTerms\"", registerMarkup);
        Assert.Contains("name=\"ConsentedPersonalDataProcessing\"", registerMarkup);
        Assert.False(Regex.IsMatch(registerMarkup, "<input(?=[^>]*name=\"(?:AcceptedTerms|ConsentedPersonalDataProcessing)\")(?=[^>]*checked)[^>]*>", RegexOptions.IgnoreCase));
    }

    [Fact]
    public async Task Registration_RequiresConsentAndStoresCurrentVersions()
    {
        using var factory = CreateFactory();
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { HandleCookies = true });
        var missingConsent = await client.PostAsJsonAsync("/api/auth/register", new
        {
            displayName = "No Consent", email = "no-consent@example.com", password = "StrongPassword123!",
            acceptedTerms = false, consentedPersonalDataProcessing = false, termsVersion = "1.0.0", privacyVersion = "1.0.0"
        });
        Assert.Equal(HttpStatusCode.BadRequest, missingConsent.StatusCode);

        using var acceptedClient = AntiforgeryTestClient.Create(factory);
        using var accepted = await acceptedClient.PostAsJsonAsync("/api/auth/register", new
        {
            displayName = "Consent Owner", email = "consent.owner@example.com", password = "StrongPassword123!",
            acceptedTerms = true, consentedPersonalDataProcessing = true, termsVersion = "1.0.0", privacyVersion = "1.0.0"
        });
        Assert.Equal(HttpStatusCode.Created, accepted.StatusCode);
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var userId = await db.Users.Where(user => user.Email == "consent.owner@example.com").Select(user => user.Id).SingleAsync();
            var entries = await db.PolicyConsents.Where(item => item.UserId == userId).ToListAsync();
            Assert.Equal(2, entries.Count);
            Assert.All(entries, item => Assert.Equal("1.0.0", item.DocumentVersion));
            Assert.All(entries, item => Assert.Equal(PolicyConsentAction.Granted, item.Action));
        }
    }

    [Fact]
    public async Task PrivacyCenter_OnlyShowsCurrentUsersRequests_AndAdminQueueIsProtected()
    {
        using var factory = CreateFactory(provisionAdmin: true);
        using var first = AntiforgeryTestClient.Create(factory);
        using var second = AntiforgeryTestClient.Create(factory);
        await Register(first, "privacy.first@example.com");
        await Register(second, "privacy.second@example.com");
        using var submitted = await first.PostAsync("/Account/Privacy/requests", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Type"] = ((int)PrivacyRequestType.Deletion).ToString(),
            ["Message"] = "private request phrase"
        }));
        Assert.Equal(HttpStatusCode.Redirect, submitted.StatusCode);
        using var firstPage = await first.GetAsync("/Account/Privacy");
        using var secondPage = await second.GetAsync("/Account/Privacy");
        using var adminQueue = await second.GetAsync("/Admin/PrivacyRequests");
        var firstMarkup = await firstPage.Content.ReadAsStringAsync();
        var secondMarkup = await secondPage.Content.ReadAsStringAsync();
        Assert.True(firstPage.StatusCode == HttpStatusCode.OK, $"Privacy page returned {(int)firstPage.StatusCode}: {firstMarkup}");
        Assert.True(secondPage.StatusCode == HttpStatusCode.OK, $"Second privacy page returned {(int)secondPage.StatusCode}: {secondMarkup}");
        Assert.Contains("private request phrase", firstMarkup);
        Assert.Contains("Điều khoản sử dụng", firstMarkup);
        Assert.Contains("1.0.0", firstMarkup);
        Assert.DoesNotContain("private request phrase", secondMarkup);
        Assert.Equal(HttpStatusCode.Redirect, adminQueue.StatusCode);

        using var admin = AntiforgeryTestClient.Create(factory);
        using var login = await admin.PostAsJsonAsync("/api/auth/login", new { email = "privacy.admin@example.com", password = "AdminPassword123!" });
        Assert.True(login.StatusCode == HttpStatusCode.OK, $"Admin login returned {(int)login.StatusCode}: {await login.Content.ReadAsStringAsync()}");
        using var queue = await admin.GetAsync("/Admin/PrivacyRequests");
        Assert.Equal(HttpStatusCode.OK, queue.StatusCode);
        Assert.Contains("private request phrase", await queue.Content.ReadAsStringAsync());
    }

    private static async Task Register(HttpClient client, string email)
    {
        using var response = await client.PostAsJsonAsync("/api/auth/register", new
        {
            displayName = "Privacy User", email, password = "StrongPassword123!",
            acceptedTerms = true, consentedPersonalDataProcessing = true, termsVersion = "1.0.0", privacyVersion = "1.0.0"
        });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    private static WebApplicationFactory<Program> CreateFactory(bool provisionAdmin = false)
    {
        var databaseName = Guid.NewGuid().ToString();
        return new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            if (provisionAdmin)
            {
                builder.ConfigureAppConfiguration((_, configuration) => configuration.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Haven:AdminBootstrap:Email"] = "privacy.admin@example.com",
                    ["Haven:AdminBootstrap:Password"] = "AdminPassword123!"
                }));
            }
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<IDbContextOptionsConfiguration<ApplicationDbContext>>();
                services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
                services.RemoveAll<ApplicationDbContext>();
                services.AddDbContext<ApplicationDbContext>(options => options.UseInMemoryDatabase(databaseName));
            });
        });
    }
}
