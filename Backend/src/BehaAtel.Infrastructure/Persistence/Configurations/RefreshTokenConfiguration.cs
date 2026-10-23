using BehaAtel.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BehaAtel.Infrastructure.Persistence.Configurations;

public sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("RefreshTokens");
        builder.ConfigureBase();
        builder.Property(token => token.UserId).IsRequired();
        builder.Property(token => token.Token).HasMaxLength(512).IsRequired();
        builder.Property(token => token.ExpiresAt).HasColumnType("timestamp with time zone").IsRequired();
        builder.Property(token => token.CreatedAt).HasColumnType("timestamp with time zone").IsRequired();
        builder.Property(token => token.RevokedAt).HasColumnType("timestamp with time zone");
        builder.Property(token => token.ReplacedByToken).HasMaxLength(512);
        builder.HasIndex(token => token.Token).IsUnique();
        builder.HasIndex(token => new { token.UserId, token.RevokedAt });
    }
}
