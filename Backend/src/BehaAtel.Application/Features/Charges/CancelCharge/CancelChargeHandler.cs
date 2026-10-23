using BehaAtel.Application.Common.Exceptions;
using BehaAtel.Application.Common.Interfaces;
using BehaAtel.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BehaAtel.Application.Features.Charges.CancelCharge;

public sealed class CancelChargeHandler(IApplicationDbContext dbContext, IAuditService auditService)
    : IRequestHandler<CancelChargeCommand>
{
    public async Task Handle(CancelChargeCommand request, CancellationToken cancellationToken)
    {
        var charge = await dbContext.Charges
            .SingleOrDefaultAsync(candidate => candidate.Id == request.ChargeId, cancellationToken)
            ?? throw new NotFoundException(nameof(Charge), request.ChargeId);

        if (await dbContext.PaymentAllocations
                .AnyAsync(allocation => allocation.ChargeId == charge.Id, cancellationToken))
        {
            throw new ConflictException("Нельзя отменить начисление, по которому уже распределены платежи.");
        }

        var oldValues = new { charge.Status, charge.Amount, charge.Period };
        charge.Cancel();
        await dbContext.SaveChangesAsync(cancellationToken);
        await auditService.LogAsync("CancelCharge", nameof(Charge), charge.Id.ToString(), oldValues,
            new { charge.Status, Reason = request.Reason.Trim() }, cancellationToken);
    }
}
