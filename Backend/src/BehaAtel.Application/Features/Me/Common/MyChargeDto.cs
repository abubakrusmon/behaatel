using BehaAtel.Domain.Enums;

namespace BehaAtel.Application.Features.Me.Common;

public sealed record MyChargeDto(
    Guid Id,
    DateOnly Period,
    ChargeType Type,
    string? Description,
    decimal Amount,
    decimal PaidAmount,
    decimal RemainingAmount,
    DateOnly DueDate,
    ChargeStatus Status);
