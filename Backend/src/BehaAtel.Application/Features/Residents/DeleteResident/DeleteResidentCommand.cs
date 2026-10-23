using MediatR;

namespace BehaAtel.Application.Features.Residents.DeleteResident;

public sealed record DeleteResidentCommand(Guid Id) : IRequest;
