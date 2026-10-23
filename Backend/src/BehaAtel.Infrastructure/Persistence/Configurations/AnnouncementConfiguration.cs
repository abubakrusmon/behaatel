using BehaAtel.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BehaAtel.Infrastructure.Persistence.Configurations;

public sealed class AnnouncementConfiguration : IEntityTypeConfiguration<Announcement>
{
    public void Configure(EntityTypeBuilder<Announcement> builder)
    {
        builder.ToTable("Announcements");
        builder.ConfigureAuditable();
        builder.Property(announcement => announcement.Title).HasMaxLength(150).IsRequired();
        builder.Property(announcement => announcement.Body).HasMaxLength(5000).IsRequired();
        builder.Property(announcement => announcement.PublishedAt).HasColumnType("timestamp with time zone");
        builder.Property(announcement => announcement.AuthorId).IsRequired();
        builder.Property(announcement => announcement.IsDeleted).IsRequired();
        builder.Property(announcement => announcement.DeletedAt).HasColumnType("timestamp with time zone");
        builder.HasIndex(announcement => announcement.PublishedAt);
        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(announcement => announcement.AuthorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
