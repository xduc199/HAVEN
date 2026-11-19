using System.Net;
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

public sealed class PublicMemberProfileTests
{
    [Fact]
    public async Task Anonymous_profile_shows_only_approved_listings_and_public_fields()
    {
        using var factory = CreateFactory();
        await SeedAsync(factory);
        using var client = factory.CreateClient();

        using var response = await client.GetAsync("/members/profile-owner");
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Public Member", html);
        Assert.Contains("Approved home", html);
        Assert.DoesNotContain("Waiting home", html);
        Assert.DoesNotContain("private.member@example.com", html);
        Assert.DoesNotContain("Admin", html);
    }

    [Fact]
    public async Task Unknown_profile_returns_not_found()
    {
        using var factory = CreateFactory();
        using var client = factory.CreateClient();
        using var response = await client.GetAsync("/members/missing");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private static async Task SeedAsync(WebApplicationFactory<Program> factory)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        db.Users.Add(new HavenUser
        {
            Id = "profile-owner",
            UserName = "private.member@example.com",
            NormalizedUserName = "PRIVATE.MEMBER@EXAMPLE.COM",
            Email = "private.member@example.com",
            NormalizedEmail = "PRIVATE.MEMBER@EXAMPLE.COM",
            DisplayName = "Public Member",
            AvatarPath = "/uploads/avatars/00000000000000000000000000000000.png"
        });
        var type = PropertyType.Create("House");
        db.PropertyTypes.Add(type);
        await db.SaveChangesAsync();
        var approved = CreateListing(type.Id, "Approved home");
        approved.SubmitForReview(DateTimeOffset.UtcNow);
        approved.Approve(DateTimeOffset.UtcNow);
        var pending = CreateListing(type.Id, "Waiting home");
        pending.SubmitForReview(DateTimeOffset.UtcNow);
        db.Listings.AddRange(approved, pending);
        await db.SaveChangesAsync();
    }

    private static Listing CreateListing(int typeId, string title) => Listing.Create(
        "profile-owner", typeId, title, "Description", ListingPurpose.Sale,
        1_000_000m, 50m, "Province", "District", "Address", DateTimeOffset.UtcNow);

    private static WebApplicationFactory<Program> CreateFactory()
    {
        var databaseName = Guid.NewGuid().ToString("N");
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
