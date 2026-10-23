using BehaAtel.Application.Features.Charges.Common;
using BehaAtel.Domain.Enums;
using MediatR;

namespace BehaAtel.Application.Features.Charges.CreateCharge;

public sealed record CreateChargeCommand(
    Guid ResidentId,
    DateOnly Period,
    ChargeType Type,
    decimal Amount,
    DateOnly DueDate,
    string? Description) : IRequest<ChargeDto>;
