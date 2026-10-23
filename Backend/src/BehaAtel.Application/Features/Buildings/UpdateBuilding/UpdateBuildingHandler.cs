using BehaAtel.Application.Common.Exceptions;
using BehaAtel.Application.Common.Interfaces;
using BehaAtel.Application.Features.Buildings.Common;
using BehaAtel.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BehaAtel.Application.Features.Buildings.UpdateBuilding;

public sealed class UpdateBuildingHandler(IApplicationDbContext dbContext, IAuditService auditService)
    : IRequestHandler<UpdateBuildingCommand, BuildingDto>
{
    public async Task<BuildingDto> Handle(UpdateBuildingCommand request, CancellationToken cancellationToken)
    {
        var building = await dbContext.Buildings
            .SingleOrDefaultAsync(candidate => candidate.Id == request.Id && !candidate.IsDeleted, cancellationToken)
            ?? throw new NotFoundException(nameof(Building), request.Id);

        var normalizedName = request.Name.Trim();
        var nameKey = normalizedName.ToLower();
        if (await dbContext.Buildings.AnyAsync(candidate =>
                candidate.Id != building.Id && !candidate.IsDeleted && candidate.Name.ToLower() == nameKey,
                cancellationToken))
        {
            throw new ConflictException("Здание с таким названием уже существует.");
        }

        if (await dbContext.Rooms.AnyAsync(room =>
                room.BuildingId == building.Id && !room.IsDeleted && room.Floor > request.FloorsCount,
                cancellationToken))
        {
            throw new ConflictException("Нельзя уменьшить количество этажей ниже этажа существующей комнаты.");
        }

        var oldValues = new { building.Name, building.Address, building.FloorsCount };
        building.Update(normalizedName, request.Address, request.FloorsCount);
        await dbContext.SaveChangesAsync(cancellationToken);
        await auditService.LogAsync("UpdateBuilding", nameof(Building), building.Id.ToString(), oldValues,
            new { building.Name, building.Address, building.FloorsCount }, cancellationToken);

        var roomTotals = await dbContext.Rooms
            .AsNoTracking()
            .Where(room => room.BuildingId == building.Id && !room.IsDeleted)
            .Select(room => new
            {
                room.Capacity,
                OccupiedBeds = room.Occupancies.Count(occupancy => occupancy.To == null)
            })
            .ToListAsync(cancellationToken);

        return new BuildingDto(building.Id, building.Name, building.Address, building.FloorsCount,
            roomTotals.Count, roomTotals.Sum(room => room.Capacity), roomTotals.Sum(room => room.OccupiedBeds));
    }
}
