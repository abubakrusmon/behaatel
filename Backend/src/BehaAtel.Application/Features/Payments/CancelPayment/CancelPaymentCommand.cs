using MediatR;

namespace BehaAtel.Application.Features.Payments.CancelPayment;

public sealed record CancelPaymentCommand(Guid PaymentId, string Reason) : IRequest;
