using BehaAtel.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BehaAtel.Infrastructure.Persistence.Configurations;

public sealed class RoomConfiguration : IEntityTypeConfiguration<Room>
{
    public void Configure(EntityTypeBuilder<Room> builder)
    {
        builder.ToTable("Rooms");
        builder.ConfigureAuditable();
        builder.Property(room => room.BuildingId).IsRequired();
        builder.Property(room => room.Number).HasMaxLength(32).IsRequired();
        builder.Property(room => room.Floor).IsRequired();
        builder.Property(room => room.Capacity).IsRequired();
        builder.Property(room => room.PricePerBed).HasPrecision(18, 2).IsRequired();
        builder.Property(room => room.Type).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(room => room.IsDeleted).IsRequired();
        builder.Property(room => room.DeletedAt).HasColumnType("timestamp with time zone");
        builder.Ignore(room => room.CurrentOccupantsCount);
        builder.Ignore(room => room.IsFull);
        builder.Ignore(room => room.FreeBeds);
        builder.HasIndex(room => new { room.BuildingId, room.Number })
            .IsUnique()
            .HasFilter("\"IsDeleted\" = FALSE");
        builder.HasMany(room => room.Occupancies)
            .WithOne()
            .HasForeignKey(occupancy => occupancy.RoomId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.Navigation(room => room.Occupancies)
            .HasField("_occupancies")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
