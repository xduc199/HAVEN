using Haven.Domain.Properties;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Haven.Infrastructure.Configurations;

public sealed class ListingConfiguration
    : IEntityTypeConfiguration<Listing>
{
    public void Configure(EntityTypeBuilder<Listing> builder)
    {
        builder.ToTable("Listings");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.OwnerId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(x => x.PropertyTypeId)
            .IsRequired();

	builder.HasOne<PropertyType>()
    .WithMany()
    .HasForeignKey(x => x.PropertyTypeId)
    .OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Description)
            .IsRequired()
            .HasMaxLength(5000);

        builder.Property(x => x.Purpose)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.PriceAmount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.AreaSqm)
            .HasPrecision(10, 2)
            .IsRequired();

        builder.Property(x => x.Province)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.District)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.AddressLine)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(x => x.Bedrooms);

        builder.Property(x => x.Bathrooms);

        builder.Property(x => x.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.CreatedAtUtc)
            .IsRequired();

        builder.Property(x => x.UpdatedAtUtc)
            .IsRequired();

        builder.Property(x => x.SubmittedAtUtc);

        builder.Property(x => x.ApprovedAtUtc);

        builder.Property(x => x.RejectionReason)
            .HasMaxLength(1000);

        builder.HasIndex(x => x.OwnerId);

        builder.HasIndex(x => new
        {
            x.Status,
            x.CreatedAtUtc
        });
    }
}
