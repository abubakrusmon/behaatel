using BehaAtel.Application.Common.Interfaces;
using BehaAtel.Application.Features.Buildings.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BehaAtel.Application.Features.Buildings.GetBuildings;

public sealed class GetBuildingsHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetBuildingsQuery, IReadOnlyCollection<BuildingDto>>
{
    public async Task<IReadOnlyCollection<BuildingDto>> Handle(GetBuildingsQuery request, CancellationToken cancellationToken) =>
        await dbContext.Buildings
            .AsNoTracking()
            .Where(building => !building.IsDeleted)
            .OrderBy(building => building.Name)
            .Select(building => new BuildingDto(
                building.Id,
                building.Name,
                building.Address,
                building.FloorsCount,
                dbContext.Rooms.Count(room => room.BuildingId == building.Id && !room.IsDeleted),
                dbContext.Rooms
                    .Where(room => room.BuildingId == building.Id && !room.IsDeleted)
                    .Sum(room => (int?)room.Capacity) ?? 0,
                (from room in dbContext.Rooms
                 where room.BuildingId == building.Id && !room.IsDeleted
                 from occupancy in dbContext.Occupancies
                 where occupancy.RoomId == room.Id && occupancy.To == null
                 select occupancy.Id).Count()))
            .ToListAsync(cancellationToken);
}
