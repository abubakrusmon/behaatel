using BehaAtel.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BehaAtel.Infrastructure.Persistence.Configurations;

public sealed class PaymentAllocationConfiguration : IEntityTypeConfiguration<PaymentAllocation>
{
    public void Configure(EntityTypeBuilder<PaymentAllocation> builder)
    {
        builder.ToTable("PaymentAllocations");
        builder.ConfigureBase();
        builder.Property(allocation => allocation.PaymentId).IsRequired();
        builder.Property(allocation => allocation.ChargeId).IsRequired();
        builder.Property(allocation => allocation.Amount).HasPrecision(18, 2).IsRequired();
        builder.HasIndex(allocation => allocation.PaymentId);
        builder.HasIndex(allocation => allocation.ChargeId);
    }
}
