using Haven.Domain.Privacy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Haven.Infrastructure.Configurations;

public sealed class PolicyConsentConfiguration : IEntityTypeConfiguration<PolicyConsent>
{
    public void Configure(EntityTypeBuilder<PolicyConsent> builder)
    {
        builder.ToTable("PolicyConsents");
        builder.HasKey(item => item.Id);
        builder.Property(item => item.UserId).HasMaxLength(PolicyConsent.UserIdMaxLength).IsRequired();
        builder.Property(item => item.DocumentKey).HasMaxLength(PolicyConsent.DocumentKeyMaxLength).IsRequired();
        builder.Property(item => item.DocumentVersion).HasMaxLength(PolicyConsent.VersionMaxLength).IsRequired();
        builder.Property(item => item.Source).HasMaxLength(PolicyConsent.SourceMaxLength).IsRequired();
        builder.Property(item => item.Action).IsRequired();
        builder.Property(item => item.OccurredAtUtc).IsRequired();
        builder.HasOne<Haven.Infrastructure.Identity.HavenUser>().WithMany().HasForeignKey(item => item.UserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(item => new { item.UserId, item.OccurredAtUtc });
    }
}
