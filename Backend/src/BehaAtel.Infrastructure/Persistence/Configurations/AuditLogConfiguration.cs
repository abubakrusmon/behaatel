using BehaAtel.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BehaAtel.Infrastructure.Persistence.Configurations;

public sealed class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLogs");
        builder.ConfigureBase();
        builder.Property(log => log.UserId);
        builder.Property(log => log.Action).HasMaxLength(100).IsRequired();
        builder.Property(log => log.EntityName).HasMaxLength(100).IsRequired();
        builder.Property(log => log.EntityId);
        builder.Property(log => log.OldValues).HasColumnType("text");
        builder.Property(log => log.NewValues).HasColumnType("text");
        builder.Property(log => log.IpAddress).HasMaxLength(64);
        builder.Property(log => log.CreatedAt).HasColumnType("timestamp with time zone").IsRequired();
        builder.HasIndex(log => log.CreatedAt);
        builder.HasIndex(log => new { log.EntityName, log.EntityId });
        builder.HasIndex(log => new { log.UserId, log.CreatedAt });
        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(log => log.UserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
