using BehaAtel.Application.Common.Exceptions;
using BehaAtel.Application.Common.Interfaces;
using BehaAtel.Application.Features.Me.Common;
using BehaAtel.Application.Features.Residents.Common;
using BehaAtel.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BehaAtel.Application.Features.Me.GetMySummary;

public sealed class GetMySummaryHandler(
    IApplicationDbContext dbContext,
    ICurrentUser currentUser,
    IDateTimeProvider dateTimeProvider)
    : IRequestHandler<GetMySummaryQuery, MySummaryDto>
{
    public async Task<MySummaryDto> Handle(GetMySummaryQuery request, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } userId)
        {
            throw new ForbiddenException("Для текущего пользователя не найдена запись жильца.");
        }

        var payments = dbContext.Payments.AsNoTracking();
        var charges = dbContext.Charges.AsNoTracking();
        var now = dateTimeProvider.UtcNow;
        var currentPeriod = new DateOnly(now.Year, now.Month, 1);
        var nextPeriod = currentPeriod.AddMonths(1);
        var summary = await dbContext.Residents.AsNoTracking()
            .Where(resident => !resident.IsDeleted && resident.UserId == userId)
            .Select(resident => new
            {
                Balance = (payments
                    .Where(payment => payment.ResidentId == resident.Id && payment.Status == PaymentStatus.Completed)
                    .Sum(payment => (decimal?)payment.Amount) ?? 0m)
                    - (charges
                        .Where(charge => charge.ResidentId == resident.Id && charge.Status != ChargeStatus.Cancelled)
                        .Sum(charge => (decimal?)charge.Amount) ?? 0m),
                DueThisMonth = charges
                    .Where(charge => charge.ResidentId == resident.Id &&
                                     charge.Period >= currentPeriod &&
                                     charge.Period < nextPeriod &&
                                     charge.Status != ChargeStatus.Cancelled)
                    .Sum(charge => (decimal?)(
                        charge.Amount -
                        (dbContext.PaymentAllocations
                            .Where(allocation => allocation.ChargeId == charge.Id)
                            .Sum(allocation => (decimal?)allocation.Amount) ?? 0m))) ?? 0m,
                TotalDebt = charges
                    .Where(charge => charge.ResidentId == resident.Id &&
                                     charge.Status != ChargeStatus.Cancelled)
                    .Sum(charge => (decimal?)(
                        charge.Amount -
                        (dbContext.PaymentAllocations
                            .Where(allocation => allocation.ChargeId == charge.Id)
                            .Sum(allocation => (decimal?)allocation.Amount) ?? 0m))) ?? 0m,
                NextDueDate = charges
                    .Where(charge => charge.ResidentId == resident.Id &&
                                     charge.Status != ChargeStatus.Cancelled &&
                                     charge.Amount >
                                     (dbContext.PaymentAllocations
                                         .Where(allocation => allocation.ChargeId == charge.Id)
                                         .Sum(allocation => (decimal?)allocation.Amount) ?? 0m))
                    .Min(charge => (DateOnly?)charge.DueDate),
                LastPayment = payments
                    .Where(payment => payment.ResidentId == resident.Id &&
                                      payment.Status == PaymentStatus.Completed)
                    .OrderByDescending(payment => payment.PaidAt)
                    .Select(payment => new MyLastPaymentDto(payment.Amount, payment.PaidAt))
                    .FirstOrDefault()
            })
            .SingleOrDefaultAsync(cancellationToken)
            ?? throw new ForbiddenException("Для текущего пользователя не найдена запись жильца.");

        return new MySummaryDto(
            summary.DueThisMonth,
            summary.Balance,
            summary.TotalDebt,
            summary.NextDueDate,
            summary.LastPayment);
    }
}
