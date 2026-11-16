using BehaAtel.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BehaAtel.Infrastructure.Persistence.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");
        builder.ConfigureAuditable();
        builder.Property(user => user.Email).HasMaxLength(320).IsRequired();
        builder.Property(user => user.PasswordHash).HasMaxLength(1024).IsRequired();
        builder.Property(user => user.FullName).HasMaxLength(200).IsRequired();
        builder.Property(user => user.Phone).HasMaxLength(32);
        builder.Property(user => user.TelegramChatId).HasMaxLength(32);
        builder.Property(user => user.AvatarData).HasColumnType("text");
        builder.Property(user => user.TelegramLinkExempt).HasDefaultValue(false).IsRequired();
        builder.Property(user => user.Role).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(user => user.IsActive).IsRequired();
        builder.Property(user => user.EmailConfirmed).IsRequired();
        builder.Property(user => user.ApprovalStatus).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(user => user.RegistrationRequestedAt).HasColumnType("timestamp with time zone");
        builder.Property(user => user.RegistrationPassportNumber).HasMaxLength(100);
        builder.Property(user => user.RegistrationBirthDate).HasColumnType("date");
        builder.Property(user => user.RegistrationOrganization).HasMaxLength(300);
        builder.Property(user => user.MustChangePassword).IsRequired();
        builder.Property(user => user.LastLoginAt).HasColumnType("timestamp with time zone");
        builder.Property(user => user.LockoutEnd).HasColumnType("timestamp with time zone");
        builder.Property(user => user.IsDeleted).IsRequired();
        builder.Property(user => user.DeletedAt).HasColumnType("timestamp with time zone");
        builder.HasIndex(user => user.Email).IsUnique().HasFilter("\"IsDeleted\" = FALSE");
        builder.HasIndex(user => user.TelegramChatId).IsUnique().HasFilter("\"TelegramChatId\" IS NOT NULL");

        builder.HasMany(user => user.Permissions)
            .WithOne()
            .HasForeignKey(permission => permission.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(user => user.Permissions)
            .HasField("_permissions")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(user => user.RefreshTokens)
            .WithOne()
            .HasForeignKey(token => token.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(user => user.RefreshTokens)
            .HasField("_refreshTokens")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
