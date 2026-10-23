using BehaAtel.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BehaAtel.Infrastructure.Persistence.Configurations;

public sealed class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("Payments");
        builder.ConfigureAuditable();
        builder.Property(payment => payment.ResidentId).IsRequired();
        builder.Property(payment => payment.Amount).HasPrecision(18, 2).IsRequired();
        builder.Property(payment => payment.Method).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(payment => payment.Status).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(payment => payment.PaidAt).HasColumnType("timestamp with time zone").IsRequired();
        builder.Property(payment => payment.ReceiptNumber).HasMaxLength(64).IsRequired();
        builder.Property(payment => payment.Comment).HasMaxLength(1000);
        builder.Property(payment => payment.CancelledReason).HasMaxLength(1000);
        builder.Property(payment => payment.CancelledAt).HasColumnType("timestamp with time zone");
        builder.Property(payment => payment.CancelledBy);
        builder.Ignore(payment => payment.AllocatedAmount);
        builder.Ignore(payment => payment.UnallocatedAmount);
        builder.HasIndex(payment => payment.ReceiptNumber).IsUnique();
        builder.HasIndex(payment => new { payment.ResidentId, payment.PaidAt });
        builder.HasIndex(payment => payment.PaidAt);
        builder.HasIndex(payment => new { payment.Status, payment.PaidAt });
        builder.HasIndex(payment => new { payment.Method, payment.Status, payment.PaidAt });
        builder.HasMany(payment => payment.Allocations)
            .WithOne()
            .HasForeignKey(allocation => allocation.PaymentId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.Navigation(payment => payment.Allocations)
            .HasField("_allocations")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(payment => payment.CancelledBy)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
