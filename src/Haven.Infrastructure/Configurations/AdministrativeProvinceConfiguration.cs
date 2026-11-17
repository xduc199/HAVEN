using Haven.Domain.Locations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Haven.Infrastructure.Configurations;

public sealed class AdministrativeProvinceConfiguration : IEntityTypeConfiguration<AdministrativeProvince>
{
    public void Configure(EntityTypeBuilder<AdministrativeProvince> builder)
    {
        builder.ToTable("AdministrativeProvinces");
        builder.HasKey(province => province.Code);
        builder.Property(province => province.Code).HasMaxLength(2).IsRequired();
        builder.Property(province => province.Name).HasMaxLength(100).IsRequired();
        builder.HasIndex(province => province.Name).IsUnique();
    }
}
