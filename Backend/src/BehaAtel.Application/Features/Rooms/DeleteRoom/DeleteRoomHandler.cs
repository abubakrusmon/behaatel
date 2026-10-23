using BehaAtel.Application.Common.Exceptions;
using BehaAtel.Application.Common.Interfaces;
using BehaAtel.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BehaAtel.Application.Features.Rooms.DeleteRoom;

public sealed class DeleteRoomHandler(IApplicationDbContext dbContext, IAuditService auditService)
    : IRequestHandler<DeleteRoomCommand>
{
    public async Task Handle(DeleteRoomCommand request, CancellationToken cancellationToken)
    {
        var room = await dbContext.Rooms
            .SingleOrDefaultAsync(candidate => candidate.Id == request.Id && !candidate.IsDeleted, cancellationToken)
            ?? throw new NotFoundException(nameof(Room), request.Id);

        if (await dbContext.Occupancies.AnyAsync(occupancy => occupancy.RoomId == room.Id && occupancy.To == null, cancellationToken))
        {
            throw new ConflictException("Нельзя удалить комнату, пока в ней проживают жильцы.");
        }

        room.MarkDeleted();
        await dbContext.SaveChangesAsync(cancellationToken);
        await auditService.LogAsync("DeleteRoom", nameof(Room), room.Id.ToString(),
            new { IsDeleted = false }, new { IsDeleted = true }, cancellationToken);
    }
}
