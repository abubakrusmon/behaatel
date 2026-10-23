using MediatR;

namespace BehaAtel.Application.Features.Residents.CheckInResident;

public sealed record CheckInResidentCommand(Guid ResidentId, Guid RoomId, DateOnly Date) : IRequest;
