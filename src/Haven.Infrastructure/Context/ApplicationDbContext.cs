using Haven.Domain.Properties;
using Haven.Domain.Consultations;
using Haven.Domain.Locations;
using Haven.Domain.Privacy;
using Haven.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace Haven.Infrastructure.Context;

public sealed class ApplicationDbContext : IdentityDbContext<HavenUser>
{
    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Listing> Listings => Set<Listing>();
    public DbSet<ListingImage> ListingImages => Set<ListingImage>();
    public DbSet<ListingVideo> ListingVideos => Set<ListingVideo>();
    public DbSet<AdministrativeProvince> AdministrativeProvinces => Set<AdministrativeProvince>();
    public DbSet<AdministrativeWard> AdministrativeWards => Set<AdministrativeWard>();
    public DbSet<PropertyType> PropertyTypes => Set<PropertyType>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<ListingTag> ListingTags => Set<ListingTag>();

    public DbSet<Favorite> Favorites => Set<Favorite>();

    public DbSet<ConsultationRequest> ConsultationRequests =>
        Set<ConsultationRequest>();
    public DbSet<PolicyConsent> PolicyConsents => Set<PolicyConsent>();
    public DbSet<PrivacyRequest> PrivacyRequests => Set<PrivacyRequest>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(ApplicationDbContext).Assembly);
    }
}
