using BehaAtel.Application.Features.Residents.Common;
using MediatR;

namespace BehaAtel.Application.Features.Residents.CreateResident;

public sealed record CreateResidentCommand(
    string Email,
    string FullName,
    string? Phone,
    string Password,
    string PassportNumber,
    DateOnly BirthDate,
    string? Organization,
    Guid? RoomId,
    DateOnly? CheckInDate) : IRequest<ResidentListItemDto>;
