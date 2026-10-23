using BehaAtel.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BehaAtel.Infrastructure.Persistence.Configurations;

public sealed class SettingConfiguration : IEntityTypeConfiguration<Setting>
{
    public void Configure(EntityTypeBuilder<Setting> builder)
    {
        builder.ToTable("Settings");
        builder.HasKey(setting => setting.Key);
        builder.Property(setting => setting.Key).HasMaxLength(100).ValueGeneratedNever();
        builder.Property(setting => setting.Value).HasMaxLength(1000).IsRequired();
        builder.Property(setting => setting.UpdatedAt).HasColumnType("timestamp with time zone").IsRequired();
    }
}
