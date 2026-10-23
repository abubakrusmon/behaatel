using BehaAtel.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BehaAtel.Infrastructure.Persistence.Configurations;

public sealed class EmailCodeConfiguration : IEntityTypeConfiguration<EmailCode>
{
    public void Configure(EntityTypeBuilder<EmailCode> builder)
    {
        builder.ToTable("EmailCodes");
        builder.ConfigureBase();
        builder.Property(code => code.UserId).IsRequired();
        builder.Property(code => code.Purpose).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(code => code.CodeHash).HasMaxLength(128).IsRequired();
        builder.Property(code => code.ExpiresAt).HasColumnType("timestamp with time zone").IsRequired();
        builder.Property(code => code.CreatedAt).HasColumnType("timestamp with time zone").IsRequired();
        builder.Property(code => code.ConsumedAt).HasColumnType("timestamp with time zone");
        builder.Property(code => code.Attempts).IsRequired();
        builder.HasIndex(code => new { code.UserId, code.Purpose });
        builder.HasIndex(code => code.ExpiresAt);
        builder.HasOne<User>().WithMany().HasForeignKey(code => code.UserId).OnDelete(DeleteBehavior.Cascade);
    }
}
