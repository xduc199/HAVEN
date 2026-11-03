using Haven.Domain.Properties;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Haven.Infrastructure.Configurations;

public sealed class FavoriteConfiguration
    : IEntityTypeConfiguration<Favorite>
{
    public void Configure(EntityTypeBuilder<Favorite> builder)
    {
        builder.ToTable("Favorites");

        builder.HasKey(favorite => new
        {
            favorite.UserId,
            favorite.ListingId
        });

        builder.Property(favorite => favorite.UserId)
            .IsRequired()
            .HasMaxLength(Favorite.UserIdMaxLength);

        builder.Property(favorite => favorite.ListingId)
            .IsRequired();

        builder.Property(favorite => favorite.CreatedAtUtc)
            .IsRequired();

        builder.HasOne<Listing>()
            .WithMany()
            .HasForeignKey(favorite => favorite.ListingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(favorite => new
        {
            favorite.UserId,
            favorite.CreatedAtUtc
        });
    }
}
