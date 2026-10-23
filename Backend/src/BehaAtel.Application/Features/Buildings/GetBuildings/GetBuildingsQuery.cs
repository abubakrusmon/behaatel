using BehaAtel.Application.Features.Buildings.Common;
using MediatR;

namespace BehaAtel.Application.Features.Buildings.GetBuildings;

public sealed record GetBuildingsQuery : IRequest<IReadOnlyCollection<BuildingDto>>;
