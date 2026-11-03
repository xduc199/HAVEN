using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Haven.Domain.Properties;
using Haven.Infrastructure.Context;
using Haven.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Haven.IntegrationTests;

public sealed class MvcWorkflowTests
{
    [Fact]
    public async Task LegacyAndInformationRoutesResolve()
    {
        using var factory = CreateFactory();
        using var client = CreateAnonymousClient(factory);

        using var legacy = await client.GetAsync("/Property?location=Hanoi");
        using var contact = await client.GetAsync("/Home/Contact");
        using var projects = await client.GetAsync("/Home/DuAn");
        using var privacy = await client.GetAsync("/Home/Privacy");
        using var home = await client.GetAsync("/");
        using var search = await client.GetAsync("/properties");
        using var compare = await client.GetAsync("/compare");

        Assert.Equal(HttpStatusCode.Redirect, legacy.StatusCode);
        Assert.StartsWith("/properties?province=Hanoi", legacy.Headers.Location?.OriginalString);
        Assert.Equal(HttpStatusCode.OK, contact.StatusCode);
        Assert.Equal(HttpStatusCode.OK, projects.StatusCode);
        Assert.Equal(HttpStatusCode.OK, privacy.StatusCode);
        Assert.Equal(HttpStatusCode.OK, home.StatusCode);
        Assert.True(
            search.StatusCode == HttpStatusCode.OK,
            $"Search returned {(int)search.StatusCode}: {await search.Content.ReadAsStringAsync()}");
        Assert.Equal(HttpStatusCode.OK, compare.StatusCode);
    }

    [Fact]
    public async Task AnonymousIsRedirectedAndUserCanUseOwnedMvcPagesButNotAdmin()
    {
        using var factory = CreateFactory();
        using var anonymous = CreateAnonymousClient(factory);

        using var protectedResponse = await anonymous.GetAsync("/profile/listings");
        using var adminResponse = await anonymous.GetAsync("/Admin");
        using var anonymousAdminApi = await anonymous.GetAsync("/api/admin/analytics");
        Assert.Equal(HttpStatusCode.Redirect, protectedResponse.StatusCode);
        Assert.StartsWith("/account/login", protectedResponse.Headers.Location?.AbsolutePath);
        Assert.Equal(HttpStatusCode.Redirect, adminResponse.StatusCode);
        Assert.StartsWith("/account/login", adminResponse.Headers.Location?.AbsolutePath);
        Assert.Equal(HttpStatusCode.Unauthorized, anonymousAdminApi.StatusCode);
        foreach (var route in AdminRoutes)
        {
            using var response = await anonymous.GetAsync(route);
            Assert.True(
                response.StatusCode == HttpStatusCode.Redirect &&
                response.Headers.Location?.AbsolutePath == "/account/login",
                $"{route} returned {(int)response.StatusCode}.");
        }

        using var user = AntiforgeryTestClient.Create(factory);
        using var registration = await user.PostAsync(
            "/account/register",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["DisplayName"] = "MVC User",
                ["Email"] = "mvc.user@example.com",
                ["Password"] = "StrongPassword123!",
                ["ConfirmPassword"] = "StrongPassword123!"
            }));
        Assert.Equal(HttpStatusCode.Redirect, registration.StatusCode);

        using var listings = await user.GetAsync("/profile/listings");
        using var deniedAdmin = await user.GetAsync("/Admin");
        using var deniedAdminApi = await user.GetAsync("/api/admin/tags");
        Assert.Equal(HttpStatusCode.OK, listings.StatusCode);
        Assert.Equal(HttpStatusCode.Redirect, deniedAdmin.StatusCode);
        Assert.StartsWith("/account/access-denied", deniedAdmin.Headers.Location?.AbsolutePath);
        Assert.Equal(HttpStatusCode.Forbidden, deniedAdminApi.StatusCode);

        foreach (var route in AdminRoutes)
        {
            using var response = await user.GetAsync(route);
            Assert.True(
                response.StatusCode == HttpStatusCode.Redirect &&
                response.Headers.Location?.AbsolutePath == "/account/access-denied",
                $"{route} returned {(int)response.StatusCode}.");
        }

        foreach (var route in new[]
        {
            "/account/profile",
            "/account/change-password",
            "/profile/listings/create",
            "/favorites",
            "/consultations",
            "/consultations/create"
        })
        {
            using var response = await user.GetAsync(route);
            Assert.True(
                response.StatusCode == HttpStatusCode.OK,
                $"{route} returned {(int)response.StatusCode}.");
        }
    }

    [Fact]
    public async Task AdminCanRenderAllAdminPages()
    {
        using var factory = CreateFactory();
        using var registrationClient = AntiforgeryTestClient.Create(factory);

        using var registration = await registrationClient.PostAsync(
            "/account/register",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["DisplayName"] = "MVC Admin",
                ["Email"] = "mvc.admin@example.com",
                ["Password"] = "StrongPassword123!",
                ["ConfirmPassword"] = "StrongPassword123!"
            }));
        Assert.Equal(HttpStatusCode.Redirect, registration.StatusCode);

        using (var scope = factory.Services.CreateScope())
        {
            var userManager = scope.ServiceProvider
                .GetRequiredService<UserManager<HavenUser>>();
            var roleManager = scope.ServiceProvider
                .GetRequiredService<RoleManager<IdentityRole>>();
            if (!await roleManager.RoleExistsAsync("Admin"))
            {
                var roleResult = await roleManager.CreateAsync(new IdentityRole("Admin"));
                Assert.True(roleResult.Succeeded);
            }

            var admin = await userManager.FindByEmailAsync("mvc.admin@example.com");
            Assert.NotNull(admin);
            var assignment = await userManager.AddToRoleAsync(admin!, "Admin");
            Assert.True(assignment.Succeeded);
        }

        using var adminClient = AntiforgeryTestClient.Create(factory);
        using var login = await adminClient.PostAsync(
            "/account/login",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["Email"] = "mvc.admin@example.com",
                ["Password"] = "StrongPassword123!",
                ["ReturnUrl"] = "/Admin/Listings"
            }));
        Assert.Equal(HttpStatusCode.Redirect, login.StatusCode);
        Assert.Equal("/Admin", login.Headers.Location?.OriginalString);

        foreach (var route in new[]
        {
            "/Admin",
            "/Admin/Dashboard",
            "/Admin/Listings",
            "/Admin/PropertyTypes",
            "/Admin/PropertyTypes/Create",
            "/Admin/Consultations",
            "/Admin/Tags",
            "/Admin/Tags/Create",
            "/Admin/Reports"
        })
        {
            using var response = await adminClient.GetAsync(route);
            Assert.True(
                response.StatusCode == HttpStatusCode.OK,
                $"{route} returned {(int)response.StatusCode}.");
            if (route == "/Admin")
            {
                var dashboardHtml = await response.Content.ReadAsStringAsync();
                Assert.Contains(
                    "Chưa có loại bất động sản đang hoạt động",
                    dashboardHtml);
                foreach (var target in new[]
                {
                    "/Admin/Listings",
                    "/Admin/PropertyTypes",
                    "/Admin/Tags",
                    "/Admin/Consultations",
                    "/Admin/Reports"
                })
                {
                    var renderedLinks = System.Text.RegularExpressions.Regex
                        .Matches(dashboardHtml, "<a[^>]+href=\"([^\"]+)\"")
                        .Select(match => match.Groups[1].Value)
                        .ToArray();
                    Assert.True(
                        renderedLinks.Any(link =>
                            string.Equals(link, target, StringComparison.OrdinalIgnoreCase) ||
                            string.Equals(link, $"{target}/Index", StringComparison.OrdinalIgnoreCase)),
                        $"Expected link to {target}; found: {string.Join(", ", renderedLinks)}");
                }
            }
        }

        using var createPropertyType = await adminClient.PostAsync(
            "/Admin/PropertyTypes/Create",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["Name"] = "Apartment",
                ["SortOrder"] = "1",
                ["IsActive"] = "true"
            }));
        Assert.Equal(HttpStatusCode.Redirect, createPropertyType.StatusCode);
        Assert.Equal("/Admin/PropertyTypes", createPropertyType.Headers.Location?.OriginalString);

        using var analytics = await adminClient.GetAsync("/api/admin/analytics");
        using var tags = await adminClient.GetAsync("/api/admin/tags");
        Assert.Equal(HttpStatusCode.OK, analytics.StatusCode);
        Assert.Equal(HttpStatusCode.OK, tags.StatusCode);
    }

    [Fact]
    public async Task ListingCreate_RejectsMissingAndInvalidAntiforgeryTokens()
    {
        using var factory = CreateFactory();
        using var client = CreateAnonymousClient(factory);

        using var tokenResponse = await client.GetAsync("/api/auth/antiforgery");
        Assert.Equal(HttpStatusCode.OK, tokenResponse.StatusCode);
        using var tokenBody = JsonDocument.Parse(
            await tokenResponse.Content.ReadAsStringAsync());
        client.DefaultRequestHeaders.Add(
            "RequestVerificationToken",
            tokenBody.RootElement.GetProperty("requestToken").GetString());

        using var registration = await client.PostAsJsonAsync(
            "/api/auth/register",
            new
            {
                displayName = "Antiforgery Listing User",
                email = "antiforgery.listing@example.com",
                password = "StrongPassword123!"
            });
        Assert.Equal(HttpStatusCode.Created, registration.StatusCode);
        client.DefaultRequestHeaders.Remove("RequestVerificationToken");

        using var refreshedToken = await client.GetAsync("/api/auth/antiforgery");
        Assert.Equal(HttpStatusCode.OK, refreshedToken.StatusCode);
        using var missingToken = await client.PostAsync(
            "/profile/listings/create",
            CreateListingForm());
        Assert.Equal(HttpStatusCode.BadRequest, missingToken.StatusCode);

        client.DefaultRequestHeaders.Add(
            "RequestVerificationToken",
            "invalid-antiforgery-token");
        using var invalidToken = await client.PostAsync(
            "/profile/listings/create",
            CreateListingForm());
        Assert.Equal(HttpStatusCode.BadRequest, invalidToken.StatusCode);

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider
            .GetRequiredService<ApplicationDbContext>();
        Assert.Empty(await dbContext.Listings.ToListAsync());
    }

    [Fact]
    public async Task UserCanCreateListing_OwnerComesFromPrincipal_AndListingStartsAsDraft()
    {
        using var factory = CreateFactory();
        using var client = AntiforgeryTestClient.Create(factory);

        using var registration = await client.PostAsync(
            "/account/register",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["DisplayName"] = "Listing Owner",
                ["Email"] = "listing.owner@example.com",
                ["Password"] = "StrongPassword123!",
                ["ConfirmPassword"] = "StrongPassword123!"
            }));
        Assert.Equal(HttpStatusCode.Redirect, registration.StatusCode);

        string ownerId;
        using (var scope = factory.Services.CreateScope())
        {
            var userManager = scope.ServiceProvider
                .GetRequiredService<UserManager<HavenUser>>();
            var user = await userManager.FindByEmailAsync("listing.owner@example.com");
            Assert.NotNull(user);
            ownerId = user!.Id;

            var dbContext = scope.ServiceProvider
                .GetRequiredService<ApplicationDbContext>();
            dbContext.PropertyTypes.Add(PropertyType.Create("Apartment"));
            await dbContext.SaveChangesAsync();
        }

        using var formPage = await client.GetAsync("/profile/listings/create");
        Assert.Equal(HttpStatusCode.OK, formPage.StatusCode);

        using var create = await client.PostAsync(
            "/profile/listings/create",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["PropertyTypeId"] = "1",
                ["Title"] = "Family home",
                ["Description"] = "Bright home near the city center.",
                ["Purpose"] = "1",
                ["PriceAmount"] = "2500000000",
                ["AreaSqm"] = "80.5",
                ["Province"] = "Hà Nội",
                ["District"] = "Cầu Giấy",
                ["AddressLine"] = "12 Test Street",
                ["Bedrooms"] = "3",
                ["Bathrooms"] = "2",
                ["OwnerId"] = "attacker-controlled-owner"
            }));

        Assert.Equal(HttpStatusCode.Redirect, create.StatusCode);
        Assert.StartsWith(
            "/profile/listings/",
            create.Headers.Location?.OriginalString);

        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider
                .GetRequiredService<ApplicationDbContext>();
            var listing = await dbContext.Listings.SingleAsync();
            Assert.Equal(ownerId, listing.OwnerId);
            Assert.Equal(ListingStatus.Draft, listing.Status);
            Assert.Equal("Family home", listing.Title);
        }

        using var listingPage = await client.GetAsync("/profile/listings");
        Assert.Equal(HttpStatusCode.OK, listingPage.StatusCode);
        Assert.Contains("Family home", await listingPage.Content.ReadAsStringAsync());

        using var invalidPropertyType = await client.PostAsync(
            "/profile/listings/create",
            CreateListingForm(propertyTypeId: 999, title: "Invalid type"));
        Assert.Equal(HttpStatusCode.OK, invalidPropertyType.StatusCode);
        Assert.Contains(
            "The selected property type does not exist or is inactive",
            await invalidPropertyType.Content.ReadAsStringAsync());

        using var invalidForm = await client.PostAsync(
            "/profile/listings/create",
            CreateListingForm(title: ""));
        Assert.Equal(HttpStatusCode.OK, invalidForm.StatusCode);
        using var verificationScope = factory.Services.CreateScope();
        var verificationDb = verificationScope.ServiceProvider
            .GetRequiredService<ApplicationDbContext>();
        Assert.Single(await verificationDb.Listings.ToListAsync());
    }

    [Fact]
    public async Task ListingCreateExplainsWhenNoActivePropertyTypesExist()
    {
        using var factory = CreateFactory();
        using var client = AntiforgeryTestClient.Create(factory);

        using var registration = await client.PostAsync(
            "/account/register",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["DisplayName"] = "No Type User",
                ["Email"] = "no.type.user@example.com",
                ["Password"] = "StrongPassword123!",
                ["ConfirmPassword"] = "StrongPassword123!"
            }));
        Assert.Equal(HttpStatusCode.Redirect, registration.StatusCode);

        using var formPage = await client.GetAsync("/profile/listings/create");
        var pageContent = await formPage.Content.ReadAsStringAsync();
        Assert.Equal(HttpStatusCode.OK, formPage.StatusCode);
        Assert.Contains("Chưa thể tạo tin đăng", pageContent);
        Assert.Contains("liên hệ Admin", pageContent);
        Assert.Contains("disabled", pageContent);

        using var create = await client.PostAsync(
            "/profile/listings/create",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["PropertyTypeId"] = "1",
                ["Title"] = "Should not be persisted",
                ["Description"] = "No active property types",
                ["Purpose"] = "1",
                ["PriceAmount"] = "1000000000",
                ["AreaSqm"] = "50",
                ["Province"] = "Hà Nội",
                ["District"] = "Ba Đình",
                ["AddressLine"] = "1 Test Street"
            }));

        Assert.Equal(HttpStatusCode.OK, create.StatusCode);
        Assert.Contains("chưa có loại bất động sản đang hoạt động", await create.Content.ReadAsStringAsync());
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider
            .GetRequiredService<ApplicationDbContext>();
        Assert.Empty(await dbContext.Listings.ToListAsync());
    }

    [Fact]
    public async Task UserLogin_DoesNotRedirectToAdminReturnUrl()
    {
        using var factory = CreateFactory();
        using var client = AntiforgeryTestClient.Create(factory);

        using var registration = await client.PostAsync(
            "/account/register",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["DisplayName"] = "Login User",
                ["Email"] = "login.user@example.com",
                ["Password"] = "StrongPassword123!",
                ["ConfirmPassword"] = "StrongPassword123!"
            }));
        Assert.Equal(HttpStatusCode.Redirect, registration.StatusCode);

        using var logout = await client.PostAsync(
            "/account/logout",
            new FormUrlEncodedContent([]));
        Assert.Equal(HttpStatusCode.Redirect, logout.StatusCode);

        using var login = await client.PostAsync(
            "/account/login",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["Email"] = "login.user@example.com",
                ["Password"] = "StrongPassword123!",
                ["ReturnUrl"] = "/Admin"
            }));

        Assert.Equal(HttpStatusCode.Redirect, login.StatusCode);
        Assert.Equal("/", login.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task StateChangingMvcRequestsRequireAntiforgeryToken()
    {
        using var factory = CreateFactory();
        using var client = CreateAnonymousClient(factory);

        using var response = await client.PostAsync(
            "/account/register",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["DisplayName"] = "No Token",
                ["Email"] = "no-token@example.com",
                ["Password"] = "StrongPassword123!",
                ["ConfirmPassword"] = "StrongPassword123!"
            }));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private static HttpClient CreateAnonymousClient(
        WebApplicationFactory<Program> factory) =>
        factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            HandleCookies = true,
            AllowAutoRedirect = false
        });

    private static readonly string[] AdminRoutes =
    [
        "/Admin",
        "/Admin/Dashboard",
        "/Admin/Listings",
        "/Admin/PropertyTypes",
        "/Admin/PropertyTypes/Create",
        "/Admin/Tags",
        "/Admin/Tags/Create",
        "/Admin/Consultations",
        "/Admin/Reports"
    ];

    private static FormUrlEncodedContent CreateListingForm(
        int propertyTypeId = 1,
        string title = "Antiforgery test listing") =>
        new(new Dictionary<string, string>
        {
            ["PropertyTypeId"] = propertyTypeId.ToString(),
            ["Title"] = title,
            ["Description"] = "Test listing",
            ["Purpose"] = "1",
            ["PriceAmount"] = "1000000000",
            ["AreaSqm"] = "50",
            ["Province"] = "Hà Nội",
            ["District"] = "Ba Đình",
            ["AddressLine"] = "1 Test Street"
        });

    private static WebApplicationFactory<Program> CreateFactory()
    {
        var databaseName = Guid.NewGuid().ToString();
        return new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<
                    IDbContextOptionsConfiguration<ApplicationDbContext>>();
                services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
                services.RemoveAll<ApplicationDbContext>();
                services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseInMemoryDatabase(databaseName));
            }));
    }
}
