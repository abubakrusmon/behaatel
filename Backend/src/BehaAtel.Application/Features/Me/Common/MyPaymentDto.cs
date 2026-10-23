using BehaAtel.Domain.Enums;

namespace BehaAtel.Application.Features.Me.Common;

public sealed record MyPaymentDto(
    Guid Id,
    decimal Amount,
    PaymentMethod Method,
    DateTime PaidAt,
    string ReceiptNumber,
    PaymentStatus Status);
