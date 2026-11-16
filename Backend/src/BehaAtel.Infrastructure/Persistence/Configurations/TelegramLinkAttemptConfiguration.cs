using BehaAtel.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BehaAtel.Infrastructure.Persistence.Configurations;

public sealed class TelegramLinkAttemptConfiguration : IEntityTypeConfiguration<TelegramLinkAttempt>
{
    public void Configure(EntityTypeBuilder<TelegramLinkAttempt> builder)
    {
        builder.ToTable("TelegramLinkAttempts");
        builder.ConfigureAuditable();
        builder.Property(attempt => attempt.StartCodeHash).HasMaxLength(64).IsRequired();
        builder.Property(attempt => attempt.PollTokenHash).HasMaxLength(64).IsRequired();
        builder.Property(attempt => attempt.ExpiresAt).HasColumnType("timestamp with time zone");
        builder.Property(attempt => attempt.BoundAt).HasColumnType("timestamp with time zone");
        builder.Property(attempt => attempt.ConsumedAt).HasColumnType("timestamp with time zone").IsConcurrencyToken();
        builder.HasIndex(attempt => attempt.StartCodeHash).IsUnique();
        builder.HasIndex(attempt => attempt.PollTokenHash).IsUnique();
        builder.HasIndex(attempt => attempt.ExpiresAt);
        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(attempt => attempt.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
