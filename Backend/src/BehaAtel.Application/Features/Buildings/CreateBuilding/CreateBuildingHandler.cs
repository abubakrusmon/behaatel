using BehaAtel.Application.Common.Exceptions;
using BehaAtel.Application.Common.Interfaces;
using BehaAtel.Application.Features.Buildings.Common;
using BehaAtel.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BehaAtel.Application.Features.Buildings.CreateBuilding;

public sealed class CreateBuildingHandler(IApplicationDbContext dbContext, IAuditService auditService)
    : IRequestHandler<CreateBuildingCommand, BuildingDto>
{
    public async Task<BuildingDto> Handle(CreateBuildingCommand request, CancellationToken cancellationToken)
    {
        var name = request.Name.Trim();
        var nameKey = name.ToLower();
        if (await dbContext.Buildings.AnyAsync(building => !building.IsDeleted && building.Name.ToLower() == nameKey, cancellationToken))
        {
            throw new ConflictException("Здание с таким названием уже существует.");
        }

        var building = Building.Create(name, request.Address, request.FloorsCount);
        dbContext.Buildings.Add(building);
        await dbContext.SaveChangesAsync(cancellationToken);
        await auditService.LogAsync("CreateBuilding", nameof(Building), building.Id.ToString(), null,
            new { building.Name, building.Address, building.FloorsCount }, cancellationToken);

        return new BuildingDto(building.Id, building.Name, building.Address, building.FloorsCount, 0, 0, 0);
    }
}
