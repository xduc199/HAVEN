using Haven.Domain.Properties;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Haven.Infrastructure.Configurations;

public sealed class ListingVideoConfiguration : IEntityTypeConfiguration<ListingVideo>
{
    public void Configure(EntityTypeBuilder<ListingVideo> builder)
    {
        builder.ToTable("ListingVideos");
        builder.HasKey(video => video.Id);
        builder.Property(video => video.StorageKey).HasMaxLength(80).IsRequired();
        builder.HasIndex(video => video.StorageKey).IsUnique();
        builder.Property(video => video.ContentType).HasMaxLength(32).IsRequired();
        builder.Property(video => video.Length).IsRequired();
        builder.Property(video => video.SortOrder).IsRequired();
        builder.Property(video => video.CreatedAtUtc).IsRequired();
        builder.HasOne(video => video.Listing).WithMany(listing => listing.Videos)
            .HasForeignKey(video => video.ListingId).OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(video => new { video.ListingId, video.SortOrder });
    }
}
