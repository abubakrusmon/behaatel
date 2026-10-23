using MediatR;

namespace BehaAtel.Application.Features.Charges.CancelCharge;

public sealed record CancelChargeCommand(Guid ChargeId, string Reason) : IRequest;
