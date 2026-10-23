using BehaAtel.Application.Common.Exceptions;
using BehaAtel.Application.Common.Interfaces;
using BehaAtel.Application.Features.Buildings.Common;
using BehaAtel.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BehaAtel.Application.Features.Buildings.GetBuildingById;

public sealed class GetBuildingByIdHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetBuildingByIdQuery, BuildingDto>
{
    public async Task<BuildingDto> Handle(GetBuildingByIdQuery request, CancellationToken cancellationToken)
    {
        var building = await dbContext.Buildings
            .AsNoTracking()
            .Where(candidate => candidate.Id == request.Id && !candidate.IsDeleted)
            .Select(candidate => new BuildingDto(
                candidate.Id,
                candidate.Name,
                candidate.Address,
                candidate.FloorsCount,
                dbContext.Rooms.Count(room => room.BuildingId == candidate.Id && !room.IsDeleted),
                dbContext.Rooms
                    .Where(room => room.BuildingId == candidate.Id && !room.IsDeleted)
                    .Sum(room => (int?)room.Capacity) ?? 0,
                (from room in dbContext.Rooms
                 where room.BuildingId == candidate.Id && !room.IsDeleted
                 from occupancy in dbContext.Occupancies
                 where occupancy.RoomId == room.Id && occupancy.To == null
                 select occupancy.Id).Count()))
            .SingleOrDefaultAsync(cancellationToken);

        return building ?? throw new NotFoundException(nameof(Building), request.Id);
    }
}
