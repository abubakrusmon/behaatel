using BehaAtel.Application.Common.Exceptions;
using BehaAtel.Application.Common.Interfaces;
using BehaAtel.Application.Features.Residents.Common;
using BehaAtel.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BehaAtel.Application.Features.Residents.CheckInResident;

public sealed class CheckInResidentHandler(
    IApplicationDbContext dbContext,
    ICurrentUser currentUser,
    IAuditService auditService)
    : IRequestHandler<CheckInResidentCommand>
{
    public async Task Handle(CheckInResidentCommand request, CancellationToken cancellationToken)
    {
        var resident = await ResidentAccess.ScopeToCurrentUser(
                dbContext.Residents.Include(candidate => candidate.Occupancies)
                    .Where(candidate => !candidate.IsDeleted), currentUser)
            .SingleOrDefaultAsync(candidate => candidate.Id == request.ResidentId, cancellationToken)
            ?? throw new NotFoundException(nameof(Resident), request.ResidentId);
        var room = await dbContext.Rooms
            .Include(candidate => candidate.Occupancies)
            .SingleOrDefaultAsync(candidate => candidate.Id == request.RoomId && !candidate.IsDeleted, cancellationToken)
            ?? throw new NotFoundException(nameof(Room), request.RoomId);

        if (!await dbContext.Buildings.AnyAsync(building =>
                building.Id == room.BuildingId && !building.IsDeleted, cancellationToken))
        {
            throw new NotFoundException(nameof(Building), room.BuildingId);
        }

        var oldValues = new { resident.Status, resident.CheckInDate, resident.CheckOutDate };
        resident.CheckIn(room, request.Date);
        await dbContext.SaveChangesAsync(cancellationToken);
        await auditService.LogAsync("CheckInResident", nameof(Resident), resident.Id.ToString(), oldValues,
            new { resident.Status, resident.CheckInDate, RoomId = room.Id, room.Number }, cancellationToken);
    }
}
