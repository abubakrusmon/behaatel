using BehaAtel.Application.Features.Buildings.Common;
using MediatR;

namespace BehaAtel.Application.Features.Buildings.GetBuildingById;

public sealed record GetBuildingByIdQuery(Guid Id) : IRequest<BuildingDto>;
