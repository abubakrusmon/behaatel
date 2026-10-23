using BehaAtel.Domain.Common;
using BehaAtel.Domain.Enums;
using BehaAtel.Domain.Exceptions;

namespace BehaAtel.Domain.Entities;

public class Payment : BaseAuditableEntity
{
    private readonly List<PaymentAllocation> _allocations = [];

    private Payment()
    {
        ReceiptNumber = string.Empty;
    }

    private Payment(Guid residentId, decimal amount, PaymentMethod method, DateTime paidAt, string receiptNumber, string? comment)
    {
        if (residentId == Guid.Empty)
        {
            throw new DomainException("ResidentId не может быть пустым.");
        }

        if (amount <= 0)
        {
            throw new DomainException("Сумма платежа должна быть больше нуля.");
        }

        if (paidAt.Kind != DateTimeKind.Utc)
        {
            throw new DomainException("PaidAt должен быть в UTC.");
        }

        if (!Enum.IsDefined(method))
        {
            throw new DomainException("Указан неизвестный способ оплаты.");
        }

        if (string.IsNullOrWhiteSpace(receiptNumber))
        {
            throw new DomainException("Номер квитанции не может быть пустым.");
        }

        ResidentId = residentId;
        Amount = amount;
        Method = method;
        Status = PaymentStatus.Completed;
        PaidAt = paidAt;
        ReceiptNumber = receiptNumber.Trim();
        Comment = string.IsNullOrWhiteSpace(comment) ? null : comment.Trim();
    }

    public Guid ResidentId { get; private set; }
    public decimal Amount { get; private set; }
    public PaymentMethod Method { get; private set; }
    public PaymentStatus Status { get; private set; }
    public DateTime PaidAt { get; private set; }
    public string ReceiptNumber { get; private set; }
    public string? Comment { get; private set; }
    public string? CancelledReason { get; private set; }
    public DateTime? CancelledAt { get; private set; }
    public Guid? CancelledBy { get; private set; }
    public IReadOnlyCollection<PaymentAllocation> Allocations => _allocations.AsReadOnly();
    public decimal AllocatedAmount => _allocations.Sum(x => x.Amount);
    public decimal UnallocatedAmount => Math.Max(0, Amount - AllocatedAmount);

    public static Payment Create(Guid residentId, decimal amount, PaymentMethod method, DateTime paidAt, string receiptNumber, string? comment = null) =>
        new(residentId, amount, method, paidAt, receiptNumber, comment);

    public void Allocate(PaymentAllocation allocation)
    {
        if (allocation is null)
        {
            throw new DomainException("Распределение платежа обязательно.");
        }

        if (Status != PaymentStatus.Completed)
        {
            throw new DomainException("Нельзя распределить отменённый платёж.");
        }

        if (allocation.PaymentId != Id)
        {
            throw new DomainException("Распределение относится к другому платежу.");
        }

        if (allocation.Amount <= 0 || allocation.Amount > UnallocatedAmount)
        {
            throw new DomainException("Сумма распределения должна быть положительной и не превышать остаток платежа.");
        }

        _allocations.Add(allocation);
    }

    public void RemoveAllocation(PaymentAllocation allocation)
    {
        if (!_allocations.Remove(allocation))
        {
            throw new DomainException("Распределение не найдено в платеже.");
        }
    }

    public void Cancel(string reason, Guid userId)
    {
        if (Status == PaymentStatus.Cancelled)
        {
            throw new DomainException("Платёж уже отменён.");
        }

        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new DomainException("Причина отмены обязательна.");
        }

        if (userId == Guid.Empty)
        {
            throw new DomainException("UserId не может быть пустым.");
        }

        Status = PaymentStatus.Cancelled;
        CancelledReason = reason.Trim();
        CancelledAt = DateTime.UtcNow;
        CancelledBy = userId;
    }
}
