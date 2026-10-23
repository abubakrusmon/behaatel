using BehaAtel.Application.Common.Interfaces;
using BehaAtel.Application.Features.Dashboard.Common;
using BehaAtel.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BehaAtel.Application.Features.Dashboard.GetPaymentsByMethod;

public sealed class GetPaymentsByMethodHandler(
    IApplicationDbContext dbContext,
    IDateTimeProvider dateTimeProvider)
    : IRequestHandler<GetPaymentsByMethodQuery, IReadOnlyCollection<MethodShareDto>>
{
    public async Task<IReadOnlyCollection<MethodShareDto>> Handle(
        GetPaymentsByMethodQuery request,
        CancellationToken cancellationToken)
    {
        var now = dateTimeProvider.UtcNow;
        var from = request.From ?? new DateTime(now.Year, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var to = request.To ?? new DateTime(now.Year + 1, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var totals = await dbContext.Payments.AsNoTracking()
            .Where(payment => payment.Status == PaymentStatus.Completed &&
                              payment.PaidAt >= from &&
                              payment.PaidAt < to)
            .GroupBy(payment => payment.Method)
            .Select(group => new
            {
                Method = group.Key,
                TotalAmount = group.Sum(payment => payment.Amount),
                Count = group.Count()
            })
            .ToListAsync(cancellationToken);

        var totalAmount = totals.Sum(item => item.TotalAmount);
        return totals
            .OrderBy(item => item.Method)
            .Select(item => new MethodShareDto(
                item.Method,
                item.TotalAmount,
                item.Count,
                totalAmount == 0
                    ? 0m
                    : Math.Round(item.TotalAmount * 100m / totalAmount, 1, MidpointRounding.AwayFromZero)))
            .ToArray();
    }
}
