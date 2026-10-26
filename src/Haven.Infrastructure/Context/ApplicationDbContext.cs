using Haven.Domain.Properties;
using Microsoft.EntityFrameworkCore;

namespace Haven.Infrastructure.Context;

public sealed class ApplicationDbContext : DbContext
{
public ApplicationDbContext(
DbContextOptions<ApplicationDbContext> options)
: base(options)
{
}
public DbSet<Listing> Listings => Set<Listing>();

public DbSet<PropertyType> PropertyTypes => Set<PropertyType>();

protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);

    modelBuilder.ApplyConfigurationsFromAssembly(
        typeof(ApplicationDbContext).Assembly);
}
}