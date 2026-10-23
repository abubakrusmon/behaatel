using MediatR;

namespace BehaAtel.Application.Features.Residents.UnblockResident;

public sealed record UnblockResidentCommand(Guid ResidentId) : IRequest;
