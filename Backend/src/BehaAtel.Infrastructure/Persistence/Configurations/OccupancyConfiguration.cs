using BehaAtel.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BehaAtel.Infrastructure.Persistence.Configurations;

public sealed class OccupancyConfiguration : IEntityTypeConfiguration<Occupancy>
{
    public void Configure(EntityTypeBuilder<Occupancy> builder)
    {
        builder.ToTable("Occupancies");
        builder.ConfigureBase();
        builder.Property(occupancy => occupancy.ResidentId).IsRequired();
        builder.Property(occupancy => occupancy.RoomId).IsRequired();
        builder.Property(occupancy => occupancy.From).HasColumnType("date").IsRequired();
        builder.Property(occupancy => occupancy.To).HasColumnType("date");
        builder.HasIndex(occupancy => new { occupancy.ResidentId, occupancy.To });
        builder.HasIndex(occupancy => new { occupancy.RoomId, occupancy.To });
    }
}
