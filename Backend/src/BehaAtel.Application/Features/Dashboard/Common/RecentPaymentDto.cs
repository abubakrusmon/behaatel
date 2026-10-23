using BehaAtel.Domain.Enums;

namespace BehaAtel.Application.Features.Dashboard.Common;

public sealed record RecentPaymentDto(
    Guid Id,
    string ResidentName,
    decimal Amount,
    PaymentMethod Method,
    DateTime PaidAt,
    string ReceiptNumber);
