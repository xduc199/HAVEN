using Haven.Domain.Properties;
using Haven.Domain.Consultations;
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
    public DbSet<PropertyType> PropertyTypes => Set<PropertyType>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<ListingTag> ListingTags => Set<ListingTag>();

    public DbSet<Favorite> Favorites => Set<Favorite>();

    public DbSet<ConsultationRequest> ConsultationRequests =>
        Set<ConsultationRequest>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(ApplicationDbContext).Assembly);
    }
}