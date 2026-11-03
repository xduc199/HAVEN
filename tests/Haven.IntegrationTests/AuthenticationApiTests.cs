using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
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

public sealed class AuthenticationApiTests
{
    [Fact]
    public async Task Register_CreatesUserSessionAndProtectsCurrentAccount()
    {
        using var factory = CreateFactory();
        using var client = CreateClient(factory);

        using var registerResponse = await client.PostAsJsonAsync(
            "/api/auth/register",
            new
            {
                displayName = "New User",
                email = "new.user@example.com",
                password = "StrongPassword123!"
            });

        Assert.Equal(HttpStatusCode.Created, registerResponse.StatusCode);
        Assert.NotNull(registerResponse.Headers.Location);
        Assert.Contains(
            registerResponse.Headers.GetValues("Set-Cookie"),
            cookie => cookie.Contains("HttpOnly", StringComparison.OrdinalIgnoreCase));

        using var registerBody = JsonDocument.Parse(
            await registerResponse.Content.ReadAsStringAsync());
        Assert.Equal(
            "new.user@example.com",
            registerBody.RootElement.GetProperty("email").GetString());
        Assert.Equal(
            "New User",
            registerBody.RootElement.GetProperty("displayName").GetString());
        Assert.Equal(
            "User",
            registerBody.RootElement.GetProperty("roles")[0].GetString());

        using var currentResponse = await client.GetAsync("/api/auth/me");

        Assert.Equal(HttpStatusCode.OK, currentResponse.StatusCode);
        using var currentBody = JsonDocument.Parse(
            await currentResponse.Content.ReadAsStringAsync());
        Assert.Equal(
            "new.user@example.com",
            currentBody.RootElement.GetProperty("email").GetString());
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
    {
        using var factory = CreateFactory();
        using var client = CreateClient(factory);

        using var response = await client.PostAsJsonAsync(
            "/api/auth/login",
            new
            {
                email = "missing.user@example.com",
                password = "StrongPassword123!"
            });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ProtectedAccountEndpoints_WithoutSession_ReturnUnauthorized()
    {
        using var factory = CreateFactory();
        using var client = CreateClient(factory);

        using var currentResponse = await client.GetAsync("/api/auth/me");
        using var logoutResponse = await client.PostAsync(
            "/api/auth/logout",
            content: null);

        Assert.Equal(HttpStatusCode.Unauthorized, currentResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, logoutResponse.StatusCode);
    }

    [Fact]
    public async Task Logout_RevokesSession()
    {
        using var factory = CreateFactory();
        using var client = CreateClient(factory);

        using var registerResponse = await client.PostAsJsonAsync(
            "/api/auth/register",
            new
            {
                displayName = "Logout User",
                email = "logout.user@example.com",
                password = "StrongPassword123!"
            });
        Assert.Equal(HttpStatusCode.Created, registerResponse.StatusCode);

        using var logoutResponse = await client.PostAsync(
            "/api/auth/logout",
            content: null);
        using var currentResponse = await client.GetAsync("/api/auth/me");

        Assert.Equal(HttpStatusCode.NoContent, logoutResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, currentResponse.StatusCode);
    }

    [Fact]
    public async Task ChangePassword_RequiresCurrentPasswordAndUpdatesCredentials()
    {
        using var factory = CreateFactory();
        using var client = CreateClient(factory);

        using var registerResponse = await client.PostAsJsonAsync(
            "/api/auth/register",
            new
            {
                displayName = "Password Change",
                email = "password.change@example.com",
                password = "StrongPassword123!"
            });
        Assert.Equal(HttpStatusCode.Created, registerResponse.StatusCode);

        using var changeResponse = await client.PostAsJsonAsync(
            "/api/auth/change-password",
            new
            {
                currentPassword = "StrongPassword123!",
                newPassword = "UpdatedPassword456!"
            });
        Assert.Equal(HttpStatusCode.NoContent, changeResponse.StatusCode);

        using var oldPasswordResponse = await client.PostAsJsonAsync(
            "/api/auth/login",
            new
            {
                email = "password.change@example.com",
                password = "StrongPassword123!"
            });
        using var newPasswordResponse = await client.PostAsJsonAsync(
            "/api/auth/login",
            new
            {
                email = "password.change@example.com",
                password = "UpdatedPassword456!"
            });

        Assert.Equal(HttpStatusCode.Unauthorized, oldPasswordResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, newPasswordResponse.StatusCode);
    }

    [Fact]
    public async Task Register_WithWeakPassword_ReturnsValidationProblem()
    {
        using var factory = CreateFactory();
        using var client = CreateClient(factory);

        using var response = await client.PostAsJsonAsync(
            "/api/auth/register",
            new
            {
                displayName = "Weak Password",
                email = "weak.password@example.com",
                password = "weak"
            });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ConfiguredAdmin_CanSignInWithBothRoles()
    {
        using var factory = CreateFactory(
            "admin@example.com",
            "AdminPassword123!");
        using var client = CreateClient(factory);

        using var response = await client.PostAsJsonAsync(
            "/api/auth/login",
            new
            {
                email = "admin@example.com",
                password = "AdminPassword123!"
            });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var body = JsonDocument.Parse(
            await response.Content.ReadAsStringAsync());
        var roles = body.RootElement.GetProperty("roles")
            .EnumerateArray()
            .Select(role => role.GetString()!)
            .ToArray();
        Assert.Equal(["Admin", "User"], roles);
    }

    [Fact]
    public async Task AccountOwner_CanUpdatePublicDisplayName()
    {
        using var factory = CreateFactory();
        using var client = CreateClient(factory);

        using var registerResponse = await client.PostAsJsonAsync(
            "/api/auth/register",
            new
            {
                displayName = "Original Name",
                email = "profile.owner@example.com",
                password = "StrongPassword123!"
            });
        Assert.Equal(HttpStatusCode.Created, registerResponse.StatusCode);

        using var updateResponse = await client.PutAsJsonAsync(
            "/api/auth/me",
            new { displayName = "Updated Name" });
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);

        using var body = JsonDocument.Parse(
            await updateResponse.Content.ReadAsStringAsync());
        Assert.Equal(
            "Updated Name",
            body.RootElement.GetProperty("displayName").GetString());
    }

    private static WebApplicationFactory<Program> CreateFactory(
        string? adminEmail = null,
        string? adminPassword = null)
    {
        return new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            if (adminEmail is not null || adminPassword is not null)
            {
                builder.ConfigureAppConfiguration((_, configuration) =>
                    configuration.AddInMemoryCollection(
                        new Dictionary<string, string?>
                        {
                            ["Haven:AdminBootstrap:Email"] = adminEmail,
                            ["Haven:AdminBootstrap:Password"] = adminPassword
                        }));
            }

            var databaseName = Guid.NewGuid().ToString();
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

    private static HttpClient CreateClient(WebApplicationFactory<Program> factory) =>
        AntiforgeryTestClient.Create(factory, new WebApplicationFactoryClientOptions
        {
            HandleCookies = true,
            AllowAutoRedirect = false
        });
}
