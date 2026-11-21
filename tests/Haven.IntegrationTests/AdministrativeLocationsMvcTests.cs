using System.Net;
using System.Net.Http.Json;
using Haven.Application.Abstractions;
using Haven.Domain.Properties;
using Haven.Infrastructure.Context;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Haven.IntegrationTests;

public sealed class AdministrativeLocationsMvcTests
{
    [Fact]
    public async Task OfficialDatasetSeedsAllProvinceAndWardCodesAndProvidesDependentLookups()
    {
        using var factory = CreateFactory();
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        var provinces = await client.GetFromJsonAsync<AdministrativeLocationOption[]>("/api/locations/provinces");
        Assert.NotNull(provinces);
        Assert.Equal(34, provinces.Length);
        Assert.Contains(provinces, item => item.Code == "01" && item.Name == "Hà Nội");

        var hanoiWards = await client.GetFromJsonAsync<AdministrativeLocationOption[]>("/api/locations/provinces/01/wards");
        Assert.NotNull(hanoiWards);
        Assert.Equal(126, hanoiWards.Length);
        Assert.Contains(hanoiWards, ward => ward.Code == "00004" && ward.Name == "Phường Ba Đình");

        using var invalid = await client.GetAsync("/api/locations/provinces/01x/wards");
        Assert.Equal(HttpStatusCode.BadRequest, invalid.StatusCode);
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        Assert.Equal(34, await db.AdministrativeProvinces.CountAsync());
        Assert.Equal(3321, await db.AdministrativeWards.CountAsync());
        Assert.Equal(3321, await db.AdministrativeWards.Select(x => x.Code).Distinct().CountAsync());
        Assert.Equal(0, await db.AdministrativeWards.CountAsync(ward => !db.AdministrativeProvinces.Any(province => province.Code == ward.ProvinceCode)));
    }

    [Fact]
    public async Task CodeBasedSearchFindsCompatibleLegacyLocationsWithoutCrossProvinceMatches()
    {
        using var factory = CreateFactory();
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var type = PropertyType.Create("Location test type");
            db.PropertyTypes.Add(type);
            await db.SaveChangesAsync();
            var legacy = Listing.Create("owner-legacy", type.Id, "Legacy Ba Dinh", "test", ListingPurpose.Sale, 1000000m, 50m, "Hà Nội", "Ba Đình", "Address", DateTimeOffset.UtcNow);
            legacy.SubmitForReview(DateTimeOffset.UtcNow);
            legacy.Approve(DateTimeOffset.UtcNow);
            var other = Listing.Create("owner-other", type.Id, "Other province listing", "test", ListingPurpose.Sale, 1000000m, 50m, "Cao Bằng", "Phường Thục Phán", "Address", DateTimeOffset.UtcNow, provinceCode: "04", wardCode: "01273");
            other.SubmitForReview(DateTimeOffset.UtcNow);
            other.Approve(DateTimeOffset.UtcNow);
            db.Listings.AddRange(legacy, other);
            await db.SaveChangesAsync();
        }

        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        using var response = await client.GetAsync("/properties?provinceCode=01&wardCode=00004");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var html = await response.Content.ReadAsStringAsync();
        Assert.Contains("data-location-province", html);
        Assert.Contains("data-location-ward", html);
        Assert.Contains("/js/listing-images.js", html);
        Assert.Contains("Legacy Ba Dinh", html);
        Assert.DoesNotContain("Other province listing", html);
    }

    private static WebApplicationFactory<Program> CreateFactory()
    {
        var databaseName = Guid.NewGuid().ToString();
        return new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<IDbContextOptionsConfiguration<ApplicationDbContext>>();
                services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
                services.RemoveAll<ApplicationDbContext>();
                services.AddDbContext<ApplicationDbContext>(options => options.UseInMemoryDatabase(databaseName));
            }));
    }
}
