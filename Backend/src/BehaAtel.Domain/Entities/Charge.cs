using BehaAtel.Domain.Common;
using BehaAtel.Domain.Enums;
using BehaAtel.Domain.Exceptions;

namespace BehaAtel.Domain.Entities;

public class Charge : BaseAuditableEntity
{
    private readonly List<PaymentAllocation> _allocations = [];

    private Charge()
    {
        Description = null;
    }

    private Charge(Guid residentId, DateOnly period, ChargeType type, decimal amount, DateOnly dueDate, string? description)
    {
        if (residentId == Guid.Empty)
        {
            throw new DomainException("ResidentId не может быть пустым.");
        }

        if (period.Day != 1)
        {
            throw new DomainException("Период начисления должен быть первым числом месяца.");
        }

        if (amount <= 0)
        {
            throw new DomainException("Сумма начисления должна быть больше нуля.");
        }

        if (!Enum.IsDefined(type))
        {
            throw new DomainException("Указан неизвестный тип начисления.");
        }

        ResidentId = residentId;
        Period = period;
        Type = type;
        Amount = amount;
        DueDate = dueDate;
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        Status = ChargeStatus.Unpaid;
    }

    public Guid ResidentId { get; private set; }
    public DateOnly Period { get; private set; }
    public ChargeType Type { get; private set; }
    public decimal Amount { get; private set; }
    public DateOnly DueDate { get; private set; }
    public string? Description { get; private set; }
    public ChargeStatus Status { get; private set; }
    public IReadOnlyCollection<PaymentAllocation> Allocations => _allocations.AsReadOnly();
    public decimal PaidAmount => _allocations.Sum(x => x.Amount);
    public decimal RemainingAmount => Math.Max(0, Amount - PaidAmount);

    public static Charge Create(Guid residentId, DateOnly period, ChargeType type, decimal amount, DateOnly dueDate, string? description = null) =>
        new(residentId, period, type, amount, dueDate, description);

    public void Allocate(PaymentAllocation allocation)
    {
        if (allocation is null)
        {
            throw new DomainException("Распределение платежа обязательно.");
        }

        if (Status == ChargeStatus.Cancelled)
        {
            throw new DomainException("Нельзя распределить платёж на отменённое начисление.");
        }

        if (allocation.ChargeId != Id)
        {
            throw new DomainException("Распределение относится к другому начислению.");
        }

        if (allocation.Amount <= 0 || allocation.Amount > RemainingAmount)
        {
            throw new DomainException("Сумма распределения должна быть положительной и не превышать остаток начисления.");
        }

        _allocations.Add(allocation);
        RecalculateStatus();
    }

    public void RemoveAllocation(PaymentAllocation allocation)
    {
        if (!_allocations.Remove(allocation))
        {
            throw new DomainException("Распределение не найдено в начислении.");
        }

        RecalculateStatus();
    }

    public void Cancel()
    {
        if (Status == ChargeStatus.Cancelled)
        {
            throw new DomainException("Начисление уже отменено.");
        }

        Status = ChargeStatus.Cancelled;
    }

    public void RecalculateStatus()
    {
        if (Status == ChargeStatus.Cancelled)
        {
            return;
        }

        Status = PaidAmount switch
        {
            <= 0 => ChargeStatus.Unpaid,
            _ when PaidAmount >= Amount => ChargeStatus.Paid,
            _ => ChargeStatus.PartiallyPaid
        };
    }
}
