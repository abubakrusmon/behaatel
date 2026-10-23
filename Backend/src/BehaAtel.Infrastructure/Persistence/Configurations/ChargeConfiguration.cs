using BehaAtel.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BehaAtel.Infrastructure.Persistence.Configurations;

public sealed class ChargeConfiguration : IEntityTypeConfiguration<Charge>
{
    public void Configure(EntityTypeBuilder<Charge> builder)
    {
        builder.ToTable("Charges");
        builder.ConfigureAuditable();
        builder.Property(charge => charge.ResidentId).IsRequired();
        builder.Property(charge => charge.Period).HasColumnType("date").IsRequired();
        builder.Property(charge => charge.Type).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(charge => charge.Amount).HasPrecision(18, 2).IsRequired();
        builder.Property(charge => charge.DueDate).HasColumnType("date").IsRequired();
        builder.Property(charge => charge.Description).HasMaxLength(1000);
        builder.Property(charge => charge.Status).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Ignore(charge => charge.PaidAmount);
        builder.Ignore(charge => charge.RemainingAmount);
        builder.HasIndex(charge => new { charge.ResidentId, charge.Period });
        builder.HasMany(charge => charge.Allocations)
            .WithOne()
            .HasForeignKey(allocation => allocation.ChargeId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.Navigation(charge => charge.Allocations)
            .HasField("_allocations")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
