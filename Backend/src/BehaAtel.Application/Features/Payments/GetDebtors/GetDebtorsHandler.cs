using BehaAtel.Application.Common.Interfaces;
using BehaAtel.Application.Common.Models;
using BehaAtel.Application.Features.Payments.Common;
using BehaAtel.Application.Features.Residents.Common;
using BehaAtel.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BehaAtel.Application.Features.Payments.GetDebtors;

public sealed class GetDebtorsHandler(IApplicationDbContext dbContext, IDateTimeProvider dateTimeProvider)
    : IRequestHandler<GetDebtorsQuery, PagedList<DebtorDto>>
{
    public async Task<PagedList<DebtorDto>> Handle(GetDebtorsQuery request, CancellationToken cancellationToken)
    {
        var residents = dbContext.Residents.AsNoTracking()
            .Where(resident => !resident.IsDeleted);
        var payments = dbContext.Payments.AsNoTracking();
        var charges = dbContext.Charges.AsNoTracking();
        var debtors = residents.WithDebt(payments, charges);

        var query =
            from resident in debtors
            join user in dbContext.Users.AsNoTracking()
                    .Where(user => !user.IsDeleted && user.Role == Role.User)
                on resident.UserId equals user.Id
            select new
            {
                ResidentId = resident.Id,
                user.FullName,
                user.Phone,
                DebtAmount = -(
                    (payments
                        .Where(payment => payment.ResidentId == resident.Id && payment.Status == PaymentStatus.Completed)
                        .Sum(payment => (decimal?)payment.Amount) ?? 0m)
                    - (charges
                        .Where(charge => charge.ResidentId == resident.Id && charge.Status != ChargeStatus.Cancelled)
                        .Sum(charge => (decimal?)charge.Amount) ?? 0m)),
                BuildingName = (
                    from occupancy in dbContext.Occupancies.AsNoTracking()
                        .Where(occupancy => occupancy.ResidentId == resident.Id && occupancy.To == null)
                    join room in dbContext.Rooms.AsNoTracking().Where(room => !room.IsDeleted)
                        on occupancy.RoomId equals room.Id
                    join building in dbContext.Buildings.AsNoTracking().Where(building => !building.IsDeleted)
                        on room.BuildingId equals building.Id
                    select building.Name).FirstOrDefault(),
                RoomNumber = (
                    from occupancy in dbContext.Occupancies.AsNoTracking()
                        .Where(occupancy => occupancy.ResidentId == resident.Id && occupancy.To == null)
                    join room in dbContext.Rooms.AsNoTracking().Where(room => !room.IsDeleted)
                        on occupancy.RoomId equals room.Id
                    select room.Number).FirstOrDefault(),
                OldestUnpaidDueDate = charges
                    .Where(charge => charge.ResidentId == resident.Id &&
                                     charge.Status != ChargeStatus.Cancelled &&
                                     charge.Amount > (dbContext.PaymentAllocations
                                         .Where(allocation => allocation.ChargeId == charge.Id)
                                         .Sum(allocation => (decimal?)allocation.Amount) ?? 0m))
                    .Min(charge => (DateOnly?)charge.DueDate)
            };

        if (!string.IsNullOrWhiteSpace(request.Pagination.Search))
        {
            var search = request.Pagination.Search.Trim().ToLower();
            query = query.Where(debtor =>
                debtor.FullName.ToLower().Contains(search) ||
                (debtor.Phone != null && debtor.Phone.ToLower().Contains(search)));
        }

        var descending = !string.Equals(request.Pagination.SortDir, "asc", StringComparison.OrdinalIgnoreCase);
        query = (request.Pagination.SortBy?.Trim().ToLower(), descending) switch
        {
            ("fullname", false) => query.OrderBy(debtor => debtor.FullName),
            ("fullname", true) => query.OrderByDescending(debtor => debtor.FullName),
            ("overdueddays", false) => query.OrderBy(debtor => debtor.OldestUnpaidDueDate),
            ("overdueddays", true) => query.OrderByDescending(debtor => debtor.OldestUnpaidDueDate),
            (_, false) => query.OrderBy(debtor => debtor.DebtAmount),
            _ => query.OrderByDescending(debtor => debtor.DebtAmount)
        };

        var totalCount = await query.CountAsync(cancellationToken);
        var debtorsPage = await query
            .Skip((request.Pagination.Page - 1) * request.Pagination.PageSize)
            .Take(request.Pagination.PageSize)
            .ToListAsync(cancellationToken);
        var today = DateOnly.FromDateTime(dateTimeProvider.UtcNow);
        var items = debtorsPage.Select(debtor =>
        {
            var overdueDays = debtor.OldestUnpaidDueDate is { } dueDate
                ? Math.Max(0, today.DayNumber - dueDate.DayNumber)
                : 0;

            return new DebtorDto(
                debtor.ResidentId,
                debtor.FullName,
                debtor.Phone,
                debtor.BuildingName,
                debtor.RoomNumber,
                debtor.DebtAmount,
                debtor.OldestUnpaidDueDate,
                overdueDays);
        });

        return PagedList<DebtorDto>.CreatePage(
            items,
            request.Pagination.Page,
            request.Pagination.PageSize,
            totalCount);
    }
}
