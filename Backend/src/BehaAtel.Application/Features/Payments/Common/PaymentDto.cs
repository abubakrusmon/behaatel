using BehaAtel.Domain.Enums;

namespace BehaAtel.Application.Features.Payments.Common;

public sealed record PaymentDto(
    Guid Id,
    Guid ResidentId,
    string ResidentName,
    decimal Amount,
    PaymentMethod Method,
    PaymentStatus Status,
    DateTime PaidAt,
    string ReceiptNumber,
    string? Comment,
    decimal AllocatedAmount,
    decimal UnallocatedAmount);
