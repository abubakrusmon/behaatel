using BehaAtel.Domain.Enums;

namespace BehaAtel.Application.Features.Dashboard.Common;

public sealed record MethodShareDto(PaymentMethod Method, decimal TotalAmount, int Count, decimal Percent);
