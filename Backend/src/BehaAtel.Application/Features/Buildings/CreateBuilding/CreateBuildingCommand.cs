using BehaAtel.Application.Features.Buildings.Common;
using MediatR;

namespace BehaAtel.Application.Features.Buildings.CreateBuilding;

public sealed record CreateBuildingCommand(string Name, string Address, int FloorsCount) : IRequest<BuildingDto>;
