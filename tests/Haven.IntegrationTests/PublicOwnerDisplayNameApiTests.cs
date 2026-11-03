using System.Net;
using System.Text.Json;
using Haven.Domain.Properties;
using Haven.Infrastructure.Context;
using Haven.Infrastructure.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Haven.IntegrationTests;

public sealed class PublicOwnerDisplayNameApiTests
{
    [Fact]
    public async Task PublicDetails_ExposeDisplayNameButNotPrivateAccountData()
    {
        using var factory = CreateFactory();
        var listingId = await SeedApprovedListingAsync(factory);
        using var client = factory.CreateClient();

        using var response = await client.GetAsync($"/api/listings/{listingId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var body = JsonDocument.Parse(
            await response.Content.ReadAsStringAsync());
        var listing = body.RootElement;
        Assert.Equal(
            "Public Display Name",
            listing.GetProperty("ownerDisplayName").GetString());
        Assert.False(listing.TryGetProperty("ownerId", out _));
        Assert.False(listing.TryGetProperty("email", out _));
    }

    private static async Task<int> SeedApprovedListingAsync(
        WebApplicationFactory<Program> factory)
    {
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider
            .GetRequiredService<ApplicationDbContext>();

        dbContext.Users.Add(new HavenUser
        {
            Id = "public-owner-id",
            UserName = "private.owner@example.com",
            NormalizedUserName = "PRIVATE.OWNER@EXAMPLE.COM",
            Email = "private.owner@example.com",
            NormalizedEmail = "PRIVATE.OWNER@EXAMPLE.COM",
            DisplayName = "Public Display Name"
        });
        var propertyType = PropertyType.Create("House");
        dbContext.PropertyTypes.Add(propertyType);
        await dbContext.SaveChangesAsync();

        var listing = Listing.Create(
            "public-owner-id",
            propertyType.Id,
            "Approved home",
            "Description",
            ListingPurpose.Sale,
            1_000_000m,
            50m,
            "Province",
            "District",
            "Address",
            DateTimeOffset.UtcNow);
        listing.SubmitForReview(DateTimeOffset.UtcNow);
        listing.Approve(DateTimeOffset.UtcNow);
        dbContext.Listings.Add(listing);
        await dbContext.SaveChangesAsync();
        return listing.Id;
    }

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
