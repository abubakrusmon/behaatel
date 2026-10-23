using BehaAtel.Application.Features.Dashboard.Common;
using MediatR;

namespace BehaAtel.Application.Features.Dashboard.GetOccupancyByBuilding;

public sealed record GetOccupancyByBuildingQuery : IRequest<IReadOnlyCollection<BuildingOccupancyDto>>;
