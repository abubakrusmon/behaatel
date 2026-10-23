using MediatR;

namespace BehaAtel.Application.Features.Residents.CheckOutResident;

public sealed record CheckOutResidentCommand(Guid ResidentId, DateOnly Date) : IRequest;
