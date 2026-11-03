using Haven.Domain.Properties;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Haven.Infrastructure.Configurations;

public sealed class ListingTagConfiguration : IEntityTypeConfiguration<ListingTag>
{
    public void Configure(EntityTypeBuilder<ListingTag> builder)
    {
        builder.ToTable("ListingTags");
        builder.HasKey(link => new { link.ListingId, link.TagId });
        builder.HasOne(link => link.Listing)
            .WithMany(listing => listing.ListingTags)
            .HasForeignKey(link => link.ListingId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(link => link.Tag)
            .WithMany(tag => tag.ListingTags)
            .HasForeignKey(link => link.TagId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(link => link.TagId);
    }
}
