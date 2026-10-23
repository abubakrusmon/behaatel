using BehaAtel.Application.Common.Interfaces;
using BehaAtel.Application.Features.Dashboard.Common;
using BehaAtel.Application.Features.Residents.Common;
using BehaAtel.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BehaAtel.Application.Features.Dashboard.GetDashboardSummary;

public sealed class GetDashboardSummaryHandler(
    IApplicationDbContext dbContext,
    IDateTimeProvider dateTimeProvider)
    : IRequestHandler<GetDashboardSummaryQuery, DashboardSummaryDto>
{
    public async Task<DashboardSummaryDto> Handle(
        GetDashboardSummaryQuery request,
        CancellationToken cancellationToken)
    {
        var now = dateTimeProvider.UtcNow;
        var thisMonth = new DateOnly(now.Year, now.Month, 1);
        var nextMonth = thisMonth.AddMonths(1);
        var lastMonth = thisMonth.AddMonths(-1);
        var thisMonthStart = ToUtcDateTime(thisMonth);
        var nextMonthStart = ToUtcDateTime(nextMonth);
        var lastMonthStart = ToUtcDateTime(lastMonth);

        var activeResidents = await dbContext.Residents.AsNoTracking()
            .CountAsync(resident => !resident.IsDeleted && resident.Status == ResidentStatus.Active, cancellationToken);
        var totalRooms = await dbContext.Rooms.AsNoTracking()
            .CountAsync(room => !room.IsDeleted, cancellationToken);
        var totalBeds = await dbContext.Rooms.AsNoTracking()
            .Where(room => !room.IsDeleted)
            .SumAsync(room => (int?)room.Capacity, cancellationToken) ?? 0;
        var occupiedBeds = await (
            from occupancy in dbContext.Occupancies.AsNoTracking().Where(occupancy => occupancy.To == null)
            join resident in dbContext.Residents.AsNoTracking().Where(resident => !resident.IsDeleted)
                on occupancy.ResidentId equals resident.Id
            join room in dbContext.Rooms.AsNoTracking().Where(room => !room.IsDeleted)
                on occupancy.RoomId equals room.Id
            select occupancy.Id)
            .CountAsync(cancellationToken);

        var payments = dbContext.Payments.AsNoTracking()
            .Where(payment => payment.Status == PaymentStatus.Completed);
        var incomeThisMonth = await payments
            .Where(payment => payment.PaidAt >= thisMonthStart && payment.PaidAt < nextMonthStart)
            .SumAsync(payment => (decimal?)payment.Amount, cancellationToken) ?? 0m;
        var incomeLastMonth = await payments
            .Where(payment => payment.PaidAt >= lastMonthStart && payment.PaidAt < thisMonthStart)
            .SumAsync(payment => (decimal?)payment.Amount, cancellationToken) ?? 0m;
        var chargedThisMonth = await dbContext.Charges.AsNoTracking()
            .Where(charge => charge.Period >= thisMonth && charge.Period < nextMonth &&
                             charge.Status != ChargeStatus.Cancelled)
            .SumAsync(charge => (decimal?)charge.Amount, cancellationToken) ?? 0m;

        var paymentsForBalance = dbContext.Payments.AsNoTracking();
        var chargesForBalance = dbContext.Charges.AsNoTracking();
        var debtors = dbContext.Residents.AsNoTracking()
            .Where(resident => !resident.IsDeleted)
            .WithDebt(paymentsForBalance, chargesForBalance);
        var debtorsCount = await debtors.CountAsync(cancellationToken);
        var totalDebt = await debtors
            .Select(resident => -(
                (paymentsForBalance
                    .Where(payment => payment.ResidentId == resident.Id && payment.Status == PaymentStatus.Completed)
                    .Sum(payment => (decimal?)payment.Amount) ?? 0m)
                - (chargesForBalance
                    .Where(charge => charge.ResidentId == resident.Id && charge.Status != ChargeStatus.Cancelled)
                    .Sum(charge => (decimal?)charge.Amount) ?? 0m)))
            .SumAsync(cancellationToken);
        var occupancyPercent = totalBeds == 0
            ? 0m
            : Math.Round((decimal)occupiedBeds * 100m / totalBeds, 1, MidpointRounding.AwayFromZero);

        return new DashboardSummaryDto(
            activeResidents,
            totalRooms,
            totalBeds,
            occupiedBeds,
            occupancyPercent,
            incomeThisMonth,
            incomeLastMonth,
            chargedThisMonth,
            totalDebt,
            debtorsCount);
    }

    private static DateTime ToUtcDateTime(DateOnly date) =>
        new(date.Year, date.Month, date.Day, 0, 0, 0, DateTimeKind.Utc);
}
