using BehaAtel.Application.Common.Exceptions;
using BehaAtel.Application.Common.Interfaces;
using BehaAtel.Application.Features.Charges.Common;
using BehaAtel.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BehaAtel.Application.Features.Charges.CreateCharge;

public sealed class CreateChargeHandler(IApplicationDbContext dbContext, IAuditService auditService)
    : IRequestHandler<CreateChargeCommand, ChargeDto>
{
    public async Task<ChargeDto> Handle(CreateChargeCommand request, CancellationToken cancellationToken)
    {
        var resident = await dbContext.Residents
            .AsNoTracking()
            .Where(candidate => candidate.Id == request.ResidentId && !candidate.IsDeleted)
            .Join(
                dbContext.Users.AsNoTracking().Where(user => !user.IsDeleted),
                candidate => candidate.UserId,
                user => user.Id,
                (candidate, user) => new { candidate.Id, user.FullName })
            .SingleOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException(nameof(Resident), request.ResidentId);

        var period = new DateOnly(request.Period.Year, request.Period.Month, 1);
        var charge = Charge.Create(
            request.ResidentId, period, request.Type, request.Amount, request.DueDate, request.Description);

        dbContext.Charges.Add(charge);
        await dbContext.SaveChangesAsync(cancellationToken);
        await auditService.LogAsync("CreateCharge", nameof(Charge), charge.Id.ToString(), null,
            new
            {
                charge.ResidentId,
                charge.Period,
                charge.Type,
                charge.Amount,
                charge.DueDate,
                charge.Description
            }, cancellationToken);

        return new ChargeDto(
            charge.Id, charge.ResidentId, resident.FullName, charge.Period, charge.Type,
            charge.Amount, charge.PaidAmount, charge.RemainingAmount, charge.DueDate, charge.Status);
    }
}
