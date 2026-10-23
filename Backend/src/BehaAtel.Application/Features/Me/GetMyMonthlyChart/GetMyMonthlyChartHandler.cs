using BehaAtel.Application.Common.Exceptions;
using BehaAtel.Application.Common.Interfaces;
using BehaAtel.Application.Features.Me.Common;
using BehaAtel.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BehaAtel.Application.Features.Me.GetMyMonthlyChart;

public sealed class GetMyMonthlyChartHandler(
    IApplicationDbContext dbContext,
    ICurrentUser currentUser,
    IDateTimeProvider dateTimeProvider)
    : IRequestHandler<GetMyMonthlyChartQuery, IReadOnlyCollection<MonthlyPointDto>>
{
    public async Task<IReadOnlyCollection<MonthlyPointDto>> Handle(
        GetMyMonthlyChartQuery request,
        CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } userId)
        {
            throw new ForbiddenException("Для текущего пользователя не найдена запись жильца.");
        }

        var residentId = await dbContext.Residents.AsNoTracking()
            .Where(resident => !resident.IsDeleted && resident.UserId == userId)
            .Select(resident => (Guid?)resident.Id)
            .SingleOrDefaultAsync(cancellationToken)
            ?? throw new ForbiddenException("Для текущего пользователя не найдена запись жильца.");

        var currentMonth = DateOnly.FromDateTime(dateTimeProvider.UtcNow);
        currentMonth = new DateOnly(currentMonth.Year, currentMonth.Month, 1);
        var firstMonth = currentMonth.AddMonths(-(request.Months - 1));
        var nextMonth = currentMonth.AddMonths(1);

        var chargedByMonth = await dbContext.Charges.AsNoTracking()
            .Where(charge => charge.ResidentId == residentId &&
                             charge.Status != ChargeStatus.Cancelled &&
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

        var fromDateTime = new DateTime(firstMonth.Year, firstMonth.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var toDateTime = new DateTime(nextMonth.Year, nextMonth.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var paidByMonth = await dbContext.Payments.AsNoTracking()
            .Where(payment => payment.ResidentId == residentId &&
                              payment.Status == PaymentStatus.Completed &&
                              payment.PaidAt >= fromDateTime &&
                              payment.PaidAt < toDateTime)
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
        var points = new List<MonthlyPointDto>(request.Months);

        for (var month = firstMonth; month <= currentMonth; month = month.AddMonths(1))
        {
            points.Add(new MonthlyPointDto(
                month.ToString("yyyy-MM", System.Globalization.CultureInfo.InvariantCulture),
                chargedLookup.GetValueOrDefault(month),
                paidLookup.GetValueOrDefault(month)));
        }

        return points;
    }
}
