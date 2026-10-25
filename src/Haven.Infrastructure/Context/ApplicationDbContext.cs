using Microsoft.EntityFrameworkCore;

namespace Haven.Infrastructure.Context;

public sealed class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
}
