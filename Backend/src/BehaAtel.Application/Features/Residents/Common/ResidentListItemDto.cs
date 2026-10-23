using BehaAtel.Domain.Enums;

namespace BehaAtel.Application.Features.Residents.Common;

public sealed record ResidentListItemDto(
    Guid Id,
    string FullName,
    string Email,
    string? Phone,
    ResidentStatus Status,
    string? BuildingName,
    string? RoomNumber,
    decimal Balance,
    bool IsActive);
