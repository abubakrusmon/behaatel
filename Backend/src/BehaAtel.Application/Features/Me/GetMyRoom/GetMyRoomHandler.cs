using BehaAtel.Application.Common.Exceptions;
using BehaAtel.Application.Common.Interfaces;
using BehaAtel.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BehaAtel.Application.Features.Me.GetMyRoom;

public sealed class GetMyRoomHandler(IApplicationDbContext dbContext, ICurrentUser currentUser)
    : IRequestHandler<GetMyRoomQuery, MyRoomDto?>
{
    public async Task<MyRoomDto?> Handle(GetMyRoomQuery request, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } userId)
        {
            throw new ForbiddenException("Для текущего пользователя не найдена запись жильца.");
        }

        var resident = await dbContext.Residents
            .AsNoTracking()
            .Where(resident => !resident.IsDeleted && resident.UserId == userId)
            .Select(resident => new { resident.Id })
            .SingleOrDefaultAsync(cancellationToken)
            ?? throw new ForbiddenException("Для текущего пользователя не найдена запись жильца.");

        var currentRoom = await (
            from occupancy in dbContext.Occupancies.AsNoTracking()
                .Where(occupancy => occupancy.ResidentId == resident.Id && occupancy.To == null)
            join room in dbContext.Rooms.AsNoTracking().Where(room => !room.IsDeleted)
                on occupancy.RoomId equals room.Id
            join building in dbContext.Buildings.AsNoTracking().Where(building => !building.IsDeleted)
                on room.BuildingId equals building.Id
            select new
            {
                room.Id,
                room.Number,
                room.Floor,
                room.Type,
                building.Name,
                building.Address
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (currentRoom is null)
        {
            return null;
        }

        var neighbors = await (
            from occupancy in dbContext.Occupancies.AsNoTracking()
                .Where(occupancy => occupancy.RoomId == currentRoom.Id &&
                                    occupancy.ResidentId != resident.Id &&
                                    occupancy.To == null)
            join otherResident in dbContext.Residents.AsNoTracking().Where(other => !other.IsDeleted)
                on occupancy.ResidentId equals otherResident.Id
            join user in dbContext.Users.AsNoTracking()
                    .Where(user => !user.IsDeleted && user.Role == Role.User)
                on otherResident.UserId equals user.Id
            orderby user.FullName
            select user.FullName)
            .ToListAsync(cancellationToken);

        return new MyRoomDto(
            currentRoom.Name,
            currentRoom.Address,
            currentRoom.Floor,
            currentRoom.Number,
            currentRoom.Type,
            neighbors);
    }
}
