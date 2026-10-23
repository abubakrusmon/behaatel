using BehaAtel.Domain.Enums;

namespace BehaAtel.Application.Features.Residents.GetResidentById;

public sealed record ResidentDetailsDto(
    Guid Id,
    Guid UserId,
    string FullName,
    string Email,
    string? Phone,
    string PassportNumber,
    DateOnly BirthDate,
    string? Organization,
    ResidentStatus Status,
    bool IsActive,
    DateOnly? CheckInDate,
    DateOnly? CheckOutDate,
    CurrentRoomDto? CurrentRoom,
    decimal Balance,
    decimal TotalCharged,
    decimal TotalPaid,
    IReadOnlyCollection<OccupancyHistoryDto> OccupancyHistory);
