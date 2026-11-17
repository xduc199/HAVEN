using Haven.Domain.Privacy;
using Haven.Domain.Properties;
using Haven.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Haven.Infrastructure.Configurations;

public sealed class PrivacyRequestConfiguration : IEntityTypeConfiguration<PrivacyRequest>
{
    public void Configure(EntityTypeBuilder<PrivacyRequest> builder)
    {
        builder.ToTable("PrivacyRequests");
        builder.HasKey(item => item.Id);
        builder.Property(item => item.UserId).HasMaxLength(PrivacyRequest.UserIdMaxLength).IsRequired();
        builder.Property(item => item.Type).IsRequired();
        builder.Property(item => item.Message).HasMaxLength(PrivacyRequest.MessageMaxLength).IsRequired();
        builder.Property(item => item.Status).IsRequired();
        builder.Property(item => item.Resolution).HasMaxLength(PrivacyRequest.ResolutionMaxLength);
        builder.Property(item => item.CreatedAtUtc).IsRequired();
        builder.Property(item => item.UpdatedAtUtc).IsRequired();
        builder.HasOne<HavenUser>().WithMany().HasForeignKey(item => item.UserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Listing>().WithMany().HasForeignKey(item => item.ListingId).OnDelete(DeleteBehavior.SetNull);
        builder.HasIndex(item => new { item.Status, item.CreatedAtUtc });
        builder.HasIndex(item => new { item.UserId, item.CreatedAtUtc });
    }
}
