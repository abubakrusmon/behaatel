using BehaAtel.Domain.Common;
using BehaAtel.Domain.Exceptions;

namespace BehaAtel.Domain.Entities;

public class PaymentAllocation : BaseEntity
{
    private PaymentAllocation()
    {
    }

    private PaymentAllocation(Guid paymentId, Guid chargeId, decimal amount)
    {
        if (paymentId == Guid.Empty || chargeId == Guid.Empty)
        {
            throw new DomainException("PaymentId и ChargeId обязательны.");
        }

        if (amount <= 0)
        {
            throw new DomainException("Сумма распределения должна быть больше нуля.");
        }

        PaymentId = paymentId;
        ChargeId = chargeId;
        Amount = amount;
    }

    public Guid PaymentId { get; private set; }
    public Guid ChargeId { get; private set; }
    public decimal Amount { get; private set; }

    public static PaymentAllocation Create(Guid paymentId, Guid chargeId, decimal amount) =>
        new(paymentId, chargeId, amount);
}
