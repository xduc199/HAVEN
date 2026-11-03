using Haven.Domain.Properties;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Haven.Infrastructure.Configurations;

public sealed class TagConfiguration : IEntityTypeConfiguration<Tag>
{
    public void Configure(EntityTypeBuilder<Tag> builder)
    {
        builder.ToTable("Tags");
        builder.HasKey(tag => tag.Id);
        builder.Property(tag => tag.Name)
            .IsRequired()
            .HasMaxLength(Tag.NameMaxLength);
        builder.Property(tag => tag.NormalizedName)
            .IsRequired()
            .HasMaxLength(Tag.NameMaxLength);
        builder.Property(tag => tag.Slug)
            .IsRequired()
            .HasMaxLength(Tag.SlugMaxLength);
        builder.Property(tag => tag.Description)
            .HasMaxLength(Tag.DescriptionMaxLength);
        builder.Property(tag => tag.IsActive).IsRequired();
        builder.Property(tag => tag.CreatedAtUtc).IsRequired();
        builder.Property(tag => tag.UpdatedAtUtc).IsRequired();
        builder.HasIndex(tag => tag.NormalizedName).IsUnique();
        builder.HasIndex(tag => tag.Slug).IsUnique();
        builder.HasIndex(tag => new { tag.IsActive, tag.Name });
    }
}
