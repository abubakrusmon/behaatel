using BehaAtel.Application.Common.Interfaces;
using BehaAtel.Application.Features.Dashboard.Common;
using BehaAtel.Application.Features.Residents.Common;
using BehaAtel.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BehaAtel.Application.Features.Dashboard.GetTopDebtors;

public sealed class GetTopDebtorsHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetTopDebtorsQuery, IReadOnlyCollection<TopDebtorDto>>
{
    public async Task<IReadOnlyCollection<TopDebtorDto>> Handle(
        GetTopDebtorsQuery request,
        CancellationToken cancellationToken)
    {
        var debtors = dbContext.Residents.AsNoTracking()
            .Where(resident => !resident.IsDeleted)
            .WithDebt(dbContext.Payments.AsNoTracking(), dbContext.Charges.AsNoTracking());
        var payments = dbContext.Payments.AsNoTracking();
        var charges = dbContext.Charges.AsNoTracking();
        return await (
            from resident in debtors
            join user in dbContext.Users.AsNoTracking().Where(user => !user.IsDeleted && user.Role == Role.User)
                on resident.UserId equals user.Id
            orderby (payments
                .Where(payment => payment.ResidentId == resident.Id && payment.Status == PaymentStatus.Completed)
                .Sum(payment => (decimal?)payment.Amount) ?? 0m)
            - (charges
                .Where(charge => charge.ResidentId == resident.Id && charge.Status != ChargeStatus.Cancelled)
                .Sum(charge => (decimal?)charge.Amount) ?? 0m)
            select new TopDebtorDto(
                resident.Id,
                user.FullName,
                (from occupancy in dbContext.Occupancies.AsNoTracking()
                    where occupancy.ResidentId == resident.Id && occupancy.To == null
                    join room in dbContext.Rooms.AsNoTracking().Where(room => !room.IsDeleted)
                        on occupancy.RoomId equals room.Id
                    join building in dbContext.Buildings.AsNoTracking().Where(building => !building.IsDeleted)
                        on room.BuildingId equals building.Id
                    select building.Name).FirstOrDefault(),
                (from occupancy in dbContext.Occupancies.AsNoTracking()
                    where occupancy.ResidentId == resident.Id && occupancy.To == null
                    join room in dbContext.Rooms.AsNoTracking().Where(room => !room.IsDeleted)
                        on occupancy.RoomId equals room.Id
                    select room.Number).FirstOrDefault(),
                -((payments
                    .Where(payment => payment.ResidentId == resident.Id && payment.Status == PaymentStatus.Completed)
                    .Sum(payment => (decimal?)payment.Amount) ?? 0m)
                  - (charges
                    .Where(charge => charge.ResidentId == resident.Id && charge.Status != ChargeStatus.Cancelled)
                    .Sum(charge => (decimal?)charge.Amount) ?? 0m))))
            .Take(request.Limit)
            .ToListAsync(cancellationToken);
    }
}
