using BehaAtel.Domain.Enums;

namespace BehaAtel.Application.Features.Charges.Common;

public sealed record ChargeDto(
    Guid Id,
    Guid ResidentId,
    string ResidentName,
    DateOnly Period,
    ChargeType Type,
    decimal Amount,
    decimal PaidAmount,
    decimal RemainingAmount,
    DateOnly DueDate,
    ChargeStatus Status);
