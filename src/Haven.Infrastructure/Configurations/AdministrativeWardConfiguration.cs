using Haven.Domain.Locations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Haven.Infrastructure.Configurations;

public sealed class AdministrativeWardConfiguration : IEntityTypeConfiguration<AdministrativeWard>
{
    public void Configure(EntityTypeBuilder<AdministrativeWard> builder)
    {
        builder.ToTable("AdministrativeWards");
        builder.HasKey(ward => ward.Code);
        builder.Property(ward => ward.Code).HasMaxLength(5).IsRequired();
        builder.Property(ward => ward.ProvinceCode).HasMaxLength(2).IsRequired();
        builder.Property(ward => ward.Name).HasMaxLength(120).IsRequired();
        builder.Property(ward => ward.Type).HasMaxLength(20).IsRequired();
        builder.HasOne(ward => ward.Province).WithMany().HasForeignKey(ward => ward.ProvinceCode).OnDelete(DeleteBehavior.Restrict);
        builder.HasAlternateKey(ward => new { ward.Code, ward.ProvinceCode });
        builder.HasIndex(ward => new { ward.ProvinceCode, ward.Name });
    }
}
