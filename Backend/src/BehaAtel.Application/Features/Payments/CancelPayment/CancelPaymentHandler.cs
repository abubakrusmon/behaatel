using BehaAtel.Application.Common.Exceptions;
using BehaAtel.Application.Common.Interfaces;
using BehaAtel.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BehaAtel.Application.Features.Payments.CancelPayment;

public sealed class CancelPaymentHandler(
    IApplicationDbContext dbContext,
    ICurrentUser currentUser,
    IAuditService auditService)
    : IRequestHandler<CancelPaymentCommand>
{
    public async Task Handle(CancelPaymentCommand request, CancellationToken cancellationToken)
    {
        var payment = await dbContext.Payments
            .Include(candidate => candidate.Allocations)
            .SingleOrDefaultAsync(candidate => candidate.Id == request.PaymentId, cancellationToken)
            ?? throw new NotFoundException(nameof(Payment), request.PaymentId);

        if (payment.Status == Domain.Enums.PaymentStatus.Cancelled)
        {
            throw new ConflictException("Платёж уже отменён.");
        }

        var allocations = payment.Allocations.ToArray();
        var charges = await dbContext.Charges
            .Include(charge => charge.Allocations)
            .Where(charge => allocations.Select(allocation => allocation.ChargeId).Contains(charge.Id))
            .ToDictionaryAsync(charge => charge.Id, cancellationToken);

        foreach (var allocation in allocations)
        {
            if (!charges.TryGetValue(allocation.ChargeId, out var charge))
            {
                throw new NotFoundException(nameof(Charge), allocation.ChargeId);
            }

            charge.RemoveAllocation(allocation);
            payment.RemoveAllocation(allocation);
            dbContext.PaymentAllocations.Remove(allocation);
        }

        var userId = currentUser.UserId;
        if (userId is null || userId == Guid.Empty)
        {
            throw new UnauthorizedException("Не удалось определить пользователя, отменяющего платёж.");
        }

        var oldValues = new { payment.Status, payment.CancelledAt, payment.CancelledBy };
        payment.Cancel(request.Reason, userId.Value);
        await dbContext.SaveChangesAsync(cancellationToken);
        await auditService.LogAsync("CancelPayment", nameof(Payment), payment.Id.ToString(), oldValues,
            new
            {
                payment.Status,
                payment.CancelledAt,
                payment.CancelledBy,
                CancelledReason = request.Reason.Trim()
            }, cancellationToken);
    }
}
