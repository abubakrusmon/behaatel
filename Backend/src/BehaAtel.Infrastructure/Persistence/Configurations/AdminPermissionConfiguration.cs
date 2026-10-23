using BehaAtel.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BehaAtel.Infrastructure.Persistence.Configurations;

public sealed class AdminPermissionConfiguration : IEntityTypeConfiguration<AdminPermission>
{
    public void Configure(EntityTypeBuilder<AdminPermission> builder)
    {
        builder.ToTable("AdminPermissions");
        builder.HasKey(permission => new { permission.UserId, permission.Permission });
        builder.Property(permission => permission.UserId).ValueGeneratedNever();
        builder.Property(permission => permission.Permission)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();
    }
}
