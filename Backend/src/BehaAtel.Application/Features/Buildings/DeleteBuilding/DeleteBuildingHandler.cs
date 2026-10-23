using BehaAtel.Application.Common.Exceptions;
using BehaAtel.Application.Common.Interfaces;
using BehaAtel.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BehaAtel.Application.Features.Buildings.DeleteBuilding;

public sealed class DeleteBuildingHandler(IApplicationDbContext dbContext, IAuditService auditService)
    : IRequestHandler<DeleteBuildingCommand>
{
    public async Task Handle(DeleteBuildingCommand request, CancellationToken cancellationToken)
    {
        var building = await dbContext.Buildings
            .SingleOrDefaultAsync(candidate => candidate.Id == request.Id && !candidate.IsDeleted, cancellationToken)
            ?? throw new NotFoundException(nameof(Building), request.Id);

        if (await dbContext.Rooms.AnyAsync(room => room.BuildingId == building.Id && !room.IsDeleted, cancellationToken))
        {
            throw new ConflictException("Нельзя удалить здание, пока в нём есть неудалённые комнаты.");
        }

        building.MarkDeleted();
        await dbContext.SaveChangesAsync(cancellationToken);
        await auditService.LogAsync("DeleteBuilding", nameof(Building), building.Id.ToString(),
            new { IsDeleted = false }, new { IsDeleted = true }, cancellationToken);
    }
}
