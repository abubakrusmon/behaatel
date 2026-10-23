using BehaAtel.Application.Common.Interfaces;
using BehaAtel.Application.Features.Dashboard.Common;
using BehaAtel.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BehaAtel.Application.Features.Dashboard.GetRevenueByMonth;

public sealed class GetRevenueByMonthHandler(
    IApplicationDbContext dbContext,
    IDateTimeProvider dateTimeProvider)
    : IRequestHandler<GetRevenueByMonthQuery, IReadOnlyCollection<RevenuePointDto>>
{
    public async Task<IReadOnlyCollection<RevenuePointDto>> Handle(
        GetRevenueByMonthQuery request,
        CancellationToken cancellationToken)
    {
        var currentMonth = DateOnly.FromDateTime(dateTimeProvider.UtcNow);
        currentMonth = new DateOnly(currentMonth.Year, currentMonth.Month, 1);
        var firstMonth = currentMonth.AddMonths(-(request.Months - 1));
        var nextMonth = currentMonth.AddMonths(1);

        var chargedByMonth = await dbContext.Charges.AsNoTracking()
            .Where(charge => charge.Status != ChargeStatus.Cancelled &&
                             charge.Period >= firstMonth &&
                             charge.Period < nextMonth)
            .GroupBy(charge => new { charge.Period.Year, charge.Period.Month })
            .Select(group => new
            {
                group.Key.Year,
                group.Key.Month,
                Amount = group.Sum(charge => charge.Amount)
            })
            .ToListAsync(cancellationToken);

        var fromUtc = ToUtcDateTime(firstMonth);
        var toUtc = ToUtcDateTime(nextMonth);
        var paidByMonth = await dbContext.Payments.AsNoTracking()
            .Where(payment => payment.Status == PaymentStatus.Completed &&
                              payment.PaidAt >= fromUtc &&
                              payment.PaidAt < toUtc)
            .GroupBy(payment => new { payment.PaidAt.Year, payment.PaidAt.Month })
            .Select(group => new
            {
                group.Key.Year,
                group.Key.Month,
                Amount = group.Sum(payment => payment.Amount)
            })
            .ToListAsync(cancellationToken);

        var chargedLookup = chargedByMonth.ToDictionary(
            item => new DateOnly(item.Year, item.Month, 1), item => item.Amount);
        var paidLookup = paidByMonth.ToDictionary(
            item => new DateOnly(item.Year, item.Month, 1), item => item.Amount);
        var result = new List<RevenuePointDto>(request.Months);
        for (var month = firstMonth; month <= currentMonth; month = month.AddMonths(1))
        {
            result.Add(new RevenuePointDto(
                month.ToString("yyyy-MM", System.Globalization.CultureInfo.InvariantCulture),
                chargedLookup.GetValueOrDefault(month),
                paidLookup.GetValueOrDefault(month)));
        }

        return result;
    }

    private static DateTime ToUtcDateTime(DateOnly date) =>
        new(date.Year, date.Month, date.Day, 0, 0, 0, DateTimeKind.Utc);
}
