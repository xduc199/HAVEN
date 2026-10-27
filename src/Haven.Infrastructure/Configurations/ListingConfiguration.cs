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
            .HasMaxLength(Listing.OwnerIdMaxLength);

        builder.Property(x => x.PropertyTypeId)
            .IsRequired();

        builder.HasOne<PropertyType>()
            .WithMany()
            .HasForeignKey(x => x.PropertyTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(Listing.TitleMaxLength);

        builder.Property(x => x.Description)
            .IsRequired()
            .HasMaxLength(Listing.DescriptionMaxLength);

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
            .HasMaxLength(Listing.ProvinceMaxLength);

        builder.Property(x => x.District)
            .IsRequired()
            .HasMaxLength(Listing.DistrictMaxLength);

        builder.Property(x => x.AddressLine)
            .IsRequired()
            .HasMaxLength(Listing.AddressLineMaxLength);

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
            .HasMaxLength(Listing.RejectionReasonMaxLength);

        builder.HasIndex(x => x.OwnerId);

        builder.HasIndex(x => new
        {
            x.Status,
            x.CreatedAtUtc
        });
    }
}
