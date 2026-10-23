using BehaAtel.Application.Common.Exceptions;
using BehaAtel.Application.Common.Interfaces;
using BehaAtel.Application.Features.Rooms.Common;
using BehaAtel.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BehaAtel.Application.Features.Rooms.GetRoomById;

public sealed class GetRoomByIdHandler(IApplicationDbContext dbContext, ICurrentUser currentUser)
    : IRequestHandler<GetRoomByIdQuery, RoomDetailsDto>
{
    public async Task<RoomDetailsDto> Handle(GetRoomByIdQuery request, CancellationToken cancellationToken)
    {
        var room = await (
            from candidate in dbContext.Rooms.AsNoTracking().Where(room => room.Id == request.Id && !room.IsDeleted)
            join building in dbContext.Buildings.AsNoTracking().Where(building => !building.IsDeleted)
                on candidate.BuildingId equals building.Id
            select new RoomDto(
                candidate.Id,
                candidate.BuildingId,
                building.Name,
                candidate.Number,
                candidate.Floor,
                candidate.Capacity,
                dbContext.Occupancies.Count(occupancy =>
                    occupancy.RoomId == candidate.Id && occupancy.To == null),
                candidate.Capacity - dbContext.Occupancies.Count(occupancy =>
                    occupancy.RoomId == candidate.Id && occupancy.To == null),
                candidate.PricePerBed,
                candidate.Type))
            .SingleOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.Room), request.Id);

        var residentsQuery =
            from occupancy in dbContext.Occupancies.AsNoTracking()
                .Where(occupancy => occupancy.RoomId == room.Id && occupancy.To == null)
            join resident in dbContext.Residents.AsNoTracking().Where(resident => !resident.IsDeleted)
                on occupancy.ResidentId equals resident.Id
            join user in dbContext.Users.AsNoTracking()
                .Where(user => !user.IsDeleted && user.Role == Role.User)
                on resident.UserId equals user.Id
            select new
            {
                    ResidentId = resident.Id,
                user.FullName,
                user.Phone,
                occupancy.From,
                    UserId = user.Id
            };

        if (currentUser.Role == Role.User)
        {
            residentsQuery = currentUser.UserId is { } userId
                    ? residentsQuery.Where(resident => resident.UserId == userId)
                : residentsQuery.Where(_ => false);
        }

        var residents = await residentsQuery
            .OrderBy(resident => resident.From)
            .ThenBy(resident => resident.FullName)
            .Select(resident => new RoomOccupantDto(resident.ResidentId, resident.FullName, resident.Phone, resident.From))
            .ToListAsync(cancellationToken);

        return new RoomDetailsDto(
            room.Id, room.BuildingId, room.BuildingName, room.Number, room.Floor, room.Capacity,
            room.OccupiedBeds, room.FreeBeds, room.PricePerBed, room.Type, residents);
    }
}
