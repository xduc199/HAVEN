using Haven.Domain.Consultations;
using Haven.Domain.Properties;
using Haven.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Haven.Infrastructure.Configurations;

public sealed class ConsultationRequestConfiguration
    : IEntityTypeConfiguration<ConsultationRequest>
{
    public void Configure(EntityTypeBuilder<ConsultationRequest> builder)
    {
        builder.ToTable("ConsultationRequests");

        builder.HasKey(request => request.Id);

        builder.Property(request => request.RequesterId)
            .IsRequired()
            .HasMaxLength(ConsultationRequest.RequesterIdMaxLength);

        builder.Property(request => request.ContactName)
            .IsRequired()
            .HasMaxLength(ConsultationRequest.NameMaxLength);

        builder.Property(request => request.Email)
            .IsRequired()
            .HasMaxLength(ConsultationRequest.EmailMaxLength);

        builder.Property(request => request.PhoneNumber)
            .IsRequired()
            .HasMaxLength(ConsultationRequest.PhoneNumberMaxLength);

        builder.Property(request => request.Message)
            .HasMaxLength(ConsultationRequest.MessageMaxLength);

        builder.Property(request => request.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(request => request.CreatedAtUtc)
            .IsRequired();

        builder.Property(request => request.UpdatedAtUtc)
            .IsRequired();

        builder.HasOne<HavenUser>()
            .WithMany()
            .HasForeignKey(request => request.RequesterId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Listing>()
            .WithMany()
            .HasForeignKey(request => request.ListingId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(request => new
        {
            request.RequesterId,
            request.CreatedAtUtc
        });

        builder.HasIndex(request => new
        {
            request.Status,
            request.CreatedAtUtc
        });
    }
}
