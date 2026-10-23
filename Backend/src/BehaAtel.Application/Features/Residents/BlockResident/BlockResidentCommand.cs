using MediatR;

namespace BehaAtel.Application.Features.Residents.BlockResident;

public sealed record BlockResidentCommand(Guid ResidentId) : IRequest;
