using BehaAtel.Application.Common.Exceptions;
using BehaAtel.Application.Common.Interfaces;
using BehaAtel.Application.Features.Residents.Common;
using BehaAtel.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BehaAtel.Application.Features.Residents.CheckOutResident;

public sealed class CheckOutResidentHandler(
    IApplicationDbContext dbContext,
    ICurrentUser currentUser,
    IAuditService auditService)
    : IRequestHandler<CheckOutResidentCommand>
{
    public async Task Handle(CheckOutResidentCommand request, CancellationToken cancellationToken)
    {
        var resident = await ResidentAccess.ScopeToCurrentUser(
                dbContext.Residents.Include(candidate => candidate.Occupancies)
                    .Where(candidate => !candidate.IsDeleted), currentUser)
            .SingleOrDefaultAsync(candidate => candidate.Id == request.ResidentId, cancellationToken)
            ?? throw new NotFoundException(nameof(Resident), request.ResidentId);
        var activeOccupancy = resident.Occupancies.SingleOrDefault(occupancy => occupancy.To is null)
            ?? throw new ConflictException("Жилец не заселён.");

        if (request.Date < activeOccupancy.From)
        {
            throw new ConflictException("Дата выселения не может быть раньше даты заселения.");
        }

        var oldValues = new { resident.Status, CheckOutDate = resident.CheckOutDate };
        resident.CheckOut(request.Date);
        await dbContext.SaveChangesAsync(cancellationToken);
        await auditService.LogAsync("CheckOutResident", nameof(Resident), resident.Id.ToString(), oldValues,
            new { resident.Status, resident.CheckOutDate }, cancellationToken);
    }
}
