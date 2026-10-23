using BehaAtel.Application.Features.Payments.Common;
using BehaAtel.Domain.Enums;
using MediatR;

namespace BehaAtel.Application.Features.Payments.CreatePayment;

public sealed record CreatePaymentCommand(
    Guid ResidentId,
    decimal Amount,
    PaymentMethod Method,
    DateTime? PaidAt = null,
    string? Comment = null) : IRequest<PaymentDto>;
