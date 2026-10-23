using BehaAtel.Application.Common.Exceptions;
using BehaAtel.Application.Common.Interfaces;
using BehaAtel.Application.Features.Rooms.Common;
using BehaAtel.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BehaAtel.Application.Features.Rooms.CreateRoom;

public sealed class CreateRoomHandler(IApplicationDbContext dbContext, IAuditService auditService)
    : IRequestHandler<CreateRoomCommand, RoomDto>
{
    public async Task<RoomDto> Handle(CreateRoomCommand request, CancellationToken cancellationToken)
    {
        var building = await dbContext.Buildings
            .AsNoTracking()
            .SingleOrDefaultAsync(candidate => candidate.Id == request.BuildingId && !candidate.IsDeleted, cancellationToken)
            ?? throw new NotFoundException(nameof(Building), request.BuildingId);

        if (request.Floor > building.FloorsCount)
        {
            throw new ConflictException("Этаж комнаты не может превышать количество этажей здания.");
        }

        var number = request.Number.Trim();
        var numberKey = number.ToLower();
        if (await dbContext.Rooms.AnyAsync(room =>
                room.BuildingId == building.Id && !room.IsDeleted && room.Number.ToLower() == numberKey,
                cancellationToken))
        {
            throw new ConflictException("Комната с таким номером уже существует в здании.");
        }

        var room = Room.Create(building.Id, number, request.Floor, request.Capacity, request.PricePerBed, request.Type);
        dbContext.Rooms.Add(room);
        await dbContext.SaveChangesAsync(cancellationToken);
        await auditService.LogAsync("CreateRoom", nameof(Room), room.Id.ToString(), null,
            new { room.BuildingId, room.Number, room.Floor, room.Capacity, room.PricePerBed, room.Type }, cancellationToken);

        return new RoomDto(room.Id, room.BuildingId, building.Name, room.Number, room.Floor, room.Capacity, 0,
            room.Capacity, room.PricePerBed, room.Type);
    }
}
