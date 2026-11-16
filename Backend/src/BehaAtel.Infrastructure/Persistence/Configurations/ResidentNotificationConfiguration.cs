using BehaAtel.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BehaAtel.Infrastructure.Persistence.Configurations;

public sealed class ResidentNotificationConfiguration : IEntityTypeConfiguration<ResidentNotification>
{
    public void Configure(EntityTypeBuilder<ResidentNotification> builder)
    {
        builder.ToTable("ResidentNotifications");
        builder.ConfigureAuditable();
        builder.Property(notification => notification.Title).HasMaxLength(150).IsRequired();
        builder.Property(notification => notification.Body).HasMaxLength(5000).IsRequired();
        builder.HasIndex(notification => new { notification.ResidentId, notification.CreatedAt });
        builder.HasOne<Resident>()
            .WithMany()
            .HasForeignKey(notification => notification.ResidentId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(notification => notification.AuthorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
