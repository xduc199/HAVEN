using Haven.Domain.Properties;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Haven.Infrastructure.Configurations;

public sealed class ListingImageConfiguration : IEntityTypeConfiguration<ListingImage>
{
    public void Configure(EntityTypeBuilder<ListingImage> builder)
    {
        builder.ToTable("ListingImages");
        builder.HasKey(image => image.Id);
        builder.Property(image => image.StorageKey).HasMaxLength(80).IsRequired();
        builder.HasIndex(image => image.StorageKey).IsUnique();
        builder.Property(image => image.ContentType).HasMaxLength(32).IsRequired();
        builder.Property(image => image.Length).IsRequired();
        builder.Property(image => image.SortOrder).IsRequired();
        builder.Property(image => image.IsPrimary).IsRequired();
        builder.Property(image => image.CreatedAtUtc).IsRequired();
        builder.HasOne(image => image.Listing)
            .WithMany(listing => listing.Images)
            .HasForeignKey(image => image.ListingId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(image => new { image.ListingId, image.SortOrder });
        builder.HasIndex(image => new { image.ListingId, image.IsPrimary });
    }
}
