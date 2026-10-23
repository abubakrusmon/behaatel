using MediatR;

namespace BehaAtel.Application.Features.Residents.GetResidentById;

public sealed record GetResidentByIdQuery(Guid Id) : IRequest<ResidentDetailsDto>;
