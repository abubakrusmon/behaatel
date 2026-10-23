using MediatR;

namespace BehaAtel.Application.Features.Buildings.DeleteBuilding;

public sealed record DeleteBuildingCommand(Guid Id) : IRequest;
