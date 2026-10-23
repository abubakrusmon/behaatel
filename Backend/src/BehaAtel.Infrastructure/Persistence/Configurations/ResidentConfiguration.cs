using BehaAtel.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BehaAtel.Infrastructure.Persistence.Configurations;

public sealed class ResidentConfiguration : IEntityTypeConfiguration<Resident>
{
    public void Configure(EntityTypeBuilder<Resident> builder)
    {
        builder.ToTable("Residents");
        builder.ConfigureAuditable();
        builder.Property(resident => resident.UserId).IsRequired();
        builder.Property(resident => resident.PassportNumber).HasMaxLength(64).IsRequired();
        builder.Property(resident => resident.BirthDate).HasColumnType("date").IsRequired();
        builder.Property(resident => resident.Organization).HasMaxLength(250);
        builder.Property(resident => resident.CheckInDate).HasColumnType("date");
        builder.Property(resident => resident.CheckOutDate).HasColumnType("date");
        builder.Property(resident => resident.Status).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(resident => resident.IsDeleted).IsRequired();
        builder.Property(resident => resident.DeletedAt).HasColumnType("timestamp with time zone");
        builder.Ignore(resident => resident.Balance);
        builder.HasIndex(resident => resident.PassportNumber).IsUnique().HasFilter("\"IsDeleted\" = FALSE");
        builder.HasIndex(resident => resident.UserId).IsUnique();
        builder.HasIndex(resident => resident.Status);

        builder.HasOne<User>()
            .WithOne()
            .HasForeignKey<Resident>(resident => resident.UserId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(resident => resident.Occupancies)
            .WithOne()
            .HasForeignKey(occupancy => occupancy.ResidentId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.Navigation(resident => resident.Occupancies)
            .HasField("_occupancies")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.HasMany(resident => resident.Charges)
            .WithOne()
            .HasForeignKey(charge => charge.ResidentId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.Navigation(resident => resident.Charges)
            .HasField("_charges")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.HasMany(resident => resident.Payments)
            .WithOne()
            .HasForeignKey(payment => payment.ResidentId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.Navigation(resident => resident.Payments)
            .HasField("_payments")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
