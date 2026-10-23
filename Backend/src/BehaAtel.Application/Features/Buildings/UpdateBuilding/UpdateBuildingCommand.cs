using BehaAtel.Application.Features.Buildings.Common;
using MediatR;

namespace BehaAtel.Application.Features.Buildings.UpdateBuilding;

public sealed record UpdateBuildingCommand(Guid Id, string Name, string Address, int FloorsCount) : IRequest<BuildingDto>;
