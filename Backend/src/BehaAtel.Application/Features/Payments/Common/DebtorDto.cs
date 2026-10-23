namespace BehaAtel.Application.Features.Payments.Common;

public sealed record DebtorDto(
    Guid ResidentId,
    string FullName,
    string? Phone,
    string? BuildingName,
    string? RoomNumber,
    decimal DebtAmount,
    DateOnly? OldestUnpaidDueDate,
    int OverdueDays);
