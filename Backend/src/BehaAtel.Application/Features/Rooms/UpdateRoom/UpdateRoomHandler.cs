using BehaAtel.Application.Common.Exceptions;
using BehaAtel.Application.Common.Interfaces;
using BehaAtel.Application.Features.Rooms.Common;
using BehaAtel.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BehaAtel.Application.Features.Rooms.UpdateRoom;

public sealed class UpdateRoomHandler(IApplicationDbContext dbContext, IAuditService auditService)
    : IRequestHandler<UpdateRoomCommand, RoomDto>
{
    public async Task<RoomDto> Handle(UpdateRoomCommand request, CancellationToken cancellationToken)
    {
        var room = await dbContext.Rooms
            .Include(candidate => candidate.Occupancies)
            .SingleOrDefaultAsync(candidate => candidate.Id == request.Id && !candidate.IsDeleted, cancellationToken)
            ?? throw new NotFoundException(nameof(Room), request.Id);
        var building = await dbContext.Buildings
            .AsNoTracking()
            .SingleOrDefaultAsync(candidate => candidate.Id == room.BuildingId && !candidate.IsDeleted, cancellationToken)
            ?? throw new NotFoundException(nameof(Building), room.BuildingId);

        if (request.Floor > building.FloorsCount)
        {
            throw new ConflictException("Этаж комнаты не может превышать количество этажей здания.");
        }

        var number = request.Number.Trim();
        var numberKey = number.ToLower();
        if (await dbContext.Rooms.AnyAsync(candidate =>
                candidate.Id != room.Id && candidate.BuildingId == room.BuildingId &&
                !candidate.IsDeleted && candidate.Number.ToLower() == numberKey,
                cancellationToken))
        {
            throw new ConflictException("Комната с таким номером уже существует в здании.");
        }

        var oldValues = new { room.Number, room.Floor, room.Capacity, room.PricePerBed, room.Type };
        room.Update(number, request.Floor, request.Capacity, request.PricePerBed, request.Type);
        await dbContext.SaveChangesAsync(cancellationToken);
        await auditService.LogAsync("UpdateRoom", nameof(Room), room.Id.ToString(), oldValues,
            new { room.Number, room.Floor, room.Capacity, room.PricePerBed, room.Type }, cancellationToken);

        return new RoomDto(room.Id, room.BuildingId, building.Name, room.Number, room.Floor, room.Capacity,
            room.CurrentOccupantsCount, room.FreeBeds, room.PricePerBed, room.Type);
    }
}
