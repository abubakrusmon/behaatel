using BehaAtel.Application.Common.Interfaces;
using BehaAtel.Application.Features.Dashboard.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BehaAtel.Application.Features.Dashboard.GetOccupancyByBuilding;

public sealed class GetOccupancyByBuildingHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetOccupancyByBuildingQuery, IReadOnlyCollection<BuildingOccupancyDto>>
{
    public async Task<IReadOnlyCollection<BuildingOccupancyDto>> Handle(
        GetOccupancyByBuildingQuery request,
        CancellationToken cancellationToken)
    {
        var buildingTotals = await dbContext.Buildings.AsNoTracking()
            .Where(building => !building.IsDeleted)
            .Select(building => new
            {
                BuildingId = building.Id,
                BuildingName = building.Name,
                TotalBeds = dbContext.Rooms
                    .Where(room => room.BuildingId == building.Id && !room.IsDeleted)
                    .Sum(room => (int?)room.Capacity) ?? 0,
                OccupiedBeds = (from room in dbContext.Rooms
                    where room.BuildingId == building.Id && !room.IsDeleted
                    from occupancy in dbContext.Occupancies
                    where occupancy.RoomId == room.Id && occupancy.To == null
                    select occupancy.Id).Count()
            })
            .OrderBy(item => item.BuildingName)
            .ToListAsync(cancellationToken);

        return buildingTotals.Select(item => new BuildingOccupancyDto(
                item.BuildingId,
                item.BuildingName,
                item.TotalBeds,
                item.OccupiedBeds,
                item.TotalBeds == 0
                    ? 0m
                    : Math.Round((decimal)item.OccupiedBeds * 100m / item.TotalBeds, 1, MidpointRounding.AwayFromZero)))
            .ToArray();
    }
}
