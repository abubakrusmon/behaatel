using BehaAtel.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BehaAtel.Infrastructure.Persistence.Configurations;

public sealed class BuildingConfiguration : IEntityTypeConfiguration<Building>
{
    public void Configure(EntityTypeBuilder<Building> builder)
    {
        builder.ToTable("Buildings");
        builder.ConfigureAuditable();
        builder.Property(building => building.Name).HasMaxLength(150).IsRequired();
        builder.Property(building => building.Address).HasMaxLength(300).IsRequired();
        builder.Property(building => building.FloorsCount).IsRequired();
        builder.Property(building => building.IsDeleted).IsRequired();
        builder.Property(building => building.DeletedAt).HasColumnType("timestamp with time zone");
        builder.HasIndex(building => building.Name).IsUnique().HasFilter("\"IsDeleted\" = FALSE");
        builder.HasMany(building => building.Rooms)
            .WithOne()
            .HasForeignKey(room => room.BuildingId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.Navigation(building => building.Rooms)
            .HasField("_rooms")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
