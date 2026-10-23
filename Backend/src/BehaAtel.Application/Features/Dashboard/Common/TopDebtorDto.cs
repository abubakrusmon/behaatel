namespace BehaAtel.Application.Features.Dashboard.Common;

public sealed record TopDebtorDto(
    Guid ResidentId,
    string FullName,
    string? BuildingName,
    string? RoomNumber,
    decimal DebtAmount);
