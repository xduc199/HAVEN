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

public sealed class PropertyTypeApiTests
{
    [Fact]
    public async Task Admin_CanManageTypes_AndPublicApiReturnsOnlyActiveTypes()
    {
        using var factory = CreateFactory();
        using var client = CreateClient(factory);
        await SignInAdminAsync(client);

        using var createHouseResponse = await client.PostAsJsonAsync(
            "/api/admin/property-types",
            new { name = "House", sortOrder = 1, isActive = true });
        Assert.Equal(HttpStatusCode.Created, createHouseResponse.StatusCode);
        Assert.NotNull(createHouseResponse.Headers.Location);
        using var houseBody = JsonDocument.Parse(
            await createHouseResponse.Content.ReadAsStringAsync());
        var houseId = houseBody.RootElement.GetProperty("id").GetInt32();

        using var duplicateResponse = await client.PostAsJsonAsync(
            "/api/admin/property-types",
            new { name = "house", sortOrder = 2, isActive = true });
        Assert.Equal(HttpStatusCode.Conflict, duplicateResponse.StatusCode);

        using var createApartmentResponse = await client.PostAsJsonAsync(
            "/api/admin/property-types",
            new { name = "Apartment", sortOrder = 2, isActive = false });
        Assert.Equal(HttpStatusCode.Created, createApartmentResponse.StatusCode);

        using var publicResponse = await client.GetAsync("/api/property-types");
        Assert.Equal(HttpStatusCode.OK, publicResponse.StatusCode);
        using var publicBody = JsonDocument.Parse(
            await publicResponse.Content.ReadAsStringAsync());
        Assert.Single(publicBody.RootElement.EnumerateArray());
        Assert.Equal(
            "House",
            publicBody.RootElement[0].GetProperty("name").GetString());

        using var updateResponse = await client.PutAsJsonAsync(
            $"/api/admin/property-types/{houseId}",
            new { name = "Homes", sortOrder = 0, isActive = false });
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);

        using var publicAfterUpdateResponse = await client.GetAsync(
            "/api/property-types");
        using var publicAfterUpdateBody = JsonDocument.Parse(
            await publicAfterUpdateResponse.Content.ReadAsStringAsync());
        Assert.Empty(publicAfterUpdateBody.RootElement.EnumerateArray());

        using var adminListResponse = await client.GetAsync(
            "/api/admin/property-types");
        Assert.Equal(HttpStatusCode.OK, adminListResponse.StatusCode);
        using var adminListBody = JsonDocument.Parse(
            await adminListResponse.Content.ReadAsStringAsync());
        Assert.Equal(2, adminListBody.RootElement.GetArrayLength());
        Assert.Equal(
            "Homes",
            adminListBody.RootElement[0].GetProperty("name").GetString());
    }

    [Fact]
    public async Task AdminPropertyTypeRoutes_RequireAdminRole()
    {
        using var factory = CreateFactory();
        using var client = CreateClient(factory);

        using var anonymousResponse = await client.GetAsync(
            "/api/admin/property-types");
        Assert.Equal(
            HttpStatusCode.Unauthorized,
            anonymousResponse.StatusCode);

        using var registrationResponse = await client.PostAsJsonAsync(
            "/api/auth/register",
            new
            {
                displayName = "Catalog User",
                email = "catalog.user@example.com",
                password = "StrongPassword123!"
            });
        Assert.Equal(HttpStatusCode.Created, registrationResponse.StatusCode);

        using var forbiddenResponse = await client.GetAsync(
            "/api/admin/property-types");
        Assert.Equal(HttpStatusCode.Forbidden, forbiddenResponse.StatusCode);
    }

    [Fact]
    public async Task AdminCreate_WithInvalidPropertyType_ReturnsBadRequest()
    {
        using var factory = CreateFactory();
        using var client = CreateClient(factory);
        await SignInAdminAsync(client);

        using var response = await client.PostAsJsonAsync(
            "/api/admin/property-types",
            new { name = "  ", sortOrder = -1, isActive = true });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
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
