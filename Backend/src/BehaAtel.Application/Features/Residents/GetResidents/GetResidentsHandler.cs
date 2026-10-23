using BehaAtel.Application.Common.Interfaces;
using BehaAtel.Application.Common.Models;
using BehaAtel.Application.Features.Residents.Common;
using BehaAtel.Domain.Entities;
using BehaAtel.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BehaAtel.Application.Features.Residents.GetResidents;

public sealed class GetResidentsHandler(IApplicationDbContext dbContext, ICurrentUser currentUser)
    : IRequestHandler<GetResidentsQuery, PagedList<ResidentListItemDto>>
{
    public async Task<PagedList<ResidentListItemDto>> Handle(GetResidentsQuery request, CancellationToken cancellationToken)
    {
        var residentQuery = ResidentAccess.ScopeToCurrentUser(
            dbContext.Residents.AsNoTracking().Where(resident => !resident.IsDeleted), currentUser);
        var paymentQuery = dbContext.Payments.AsNoTracking();
        var chargeQuery = dbContext.Charges.AsNoTracking();
        var residentsWithBalance = request.HasDebt switch
        {
            true => residentQuery.WithDebt(paymentQuery, chargeQuery),
            false => residentQuery.WithNoDebt(paymentQuery, chargeQuery),
            _ => residentQuery
        };
        if (request.Status.HasValue)
        {
            residentsWithBalance = residentsWithBalance.Where(resident => resident.Status == request.Status.Value);
        }

        if (request.BuildingId.HasValue)
        {
            residentsWithBalance = residentsWithBalance.Where(resident =>
                (from occupancy in dbContext.Occupancies.AsNoTracking()
                    where occupancy.ResidentId == resident.Id && occupancy.To == null
                    join room in dbContext.Rooms.AsNoTracking().Where(room => !room.IsDeleted)
                        on occupancy.RoomId equals room.Id
                    select room.BuildingId).Any(buildingId => buildingId == request.BuildingId.Value));
        }

        if (request.RoomId.HasValue)
        {
            residentsWithBalance = residentsWithBalance.Where(resident =>
                dbContext.Occupancies.AsNoTracking().Any(occupancy =>
                    occupancy.ResidentId == resident.Id &&
                    occupancy.To == null &&
                    occupancy.RoomId == request.RoomId.Value));
        }

        var query =
            from resident in residentsWithBalance
            join user in dbContext.Users.AsNoTracking().Where(user => !user.IsDeleted && user.Role == Role.User)
                on resident.UserId equals user.Id
            select new
            {
                Resident = resident,
                User = user,
                BuildingName = (from occupancy in dbContext.Occupancies.AsNoTracking()
                    where occupancy.ResidentId == resident.Id && occupancy.To == null
                    join room in dbContext.Rooms.AsNoTracking().Where(room => !room.IsDeleted)
                        on occupancy.RoomId equals room.Id
                    join building in dbContext.Buildings.AsNoTracking().Where(building => !building.IsDeleted)
                        on room.BuildingId equals building.Id
                    select building.Name).FirstOrDefault(),
                RoomNumber = (from occupancy in dbContext.Occupancies.AsNoTracking()
                    where occupancy.ResidentId == resident.Id && occupancy.To == null
                    join room in dbContext.Rooms.AsNoTracking().Where(room => !room.IsDeleted)
                        on occupancy.RoomId equals room.Id
                    select room.Number).FirstOrDefault()
            };

        if (!string.IsNullOrWhiteSpace(request.Pagination.Search))
        {
            var search = request.Pagination.Search.Trim().ToLower();
            query = query.Where(resident =>
                resident.User.FullName.ToLower().Contains(search) ||
                resident.User.Email.ToLower().Contains(search) ||
                (resident.User.Phone != null && resident.User.Phone.ToLower().Contains(search)) ||
                resident.Resident.PassportNumber.ToLower().Contains(search));
        }

        var descending = string.Equals(request.Pagination.SortDir, "desc", StringComparison.OrdinalIgnoreCase);
        query = (request.Pagination.SortBy?.Trim().ToLower(), descending) switch
        {
            ("balance", false) => query.OrderBy(resident =>
                (paymentQuery
                    .Where(payment => payment.ResidentId == resident.Resident.Id && payment.Status == PaymentStatus.Completed)
                    .Sum(payment => (decimal?)payment.Amount) ?? 0m)
                - (chargeQuery
                    .Where(charge => charge.ResidentId == resident.Resident.Id && charge.Status != ChargeStatus.Cancelled)
                    .Sum(charge => (decimal?)charge.Amount) ?? 0m)),
            ("balance", true) => query.OrderByDescending(resident =>
                (paymentQuery
                    .Where(payment => payment.ResidentId == resident.Resident.Id && payment.Status == PaymentStatus.Completed)
                    .Sum(payment => (decimal?)payment.Amount) ?? 0m)
                - (chargeQuery
                    .Where(charge => charge.ResidentId == resident.Resident.Id && charge.Status != ChargeStatus.Cancelled)
                    .Sum(charge => (decimal?)charge.Amount) ?? 0m)),
            ("checkindate", false) => query.OrderBy(resident => resident.Resident.CheckInDate),
            ("checkindate", true) => query.OrderByDescending(resident => resident.Resident.CheckInDate),
            ("fullname", true) => query.OrderByDescending(resident => resident.User.FullName),
            (_, false) => query.OrderBy(resident => resident.User.FullName),
            _ => query.OrderByDescending(resident => resident.User.FullName)
        };

        return await PagedList<ResidentListItemDto>.CreateAsync(
            query.Select(resident => new ResidentListItemDto(
                resident.Resident.Id,
                resident.User.FullName,
                resident.User.Email,
                resident.User.Phone,
                resident.Resident.Status,
                resident.BuildingName,
                resident.RoomNumber,
                (paymentQuery
                    .Where(payment => payment.ResidentId == resident.Resident.Id && payment.Status == PaymentStatus.Completed)
                    .Sum(payment => (decimal?)payment.Amount) ?? 0m)
                - (chargeQuery
                    .Where(charge => charge.ResidentId == resident.Resident.Id && charge.Status != ChargeStatus.Cancelled)
                    .Sum(charge => (decimal?)charge.Amount) ?? 0m),
                resident.User.IsActive)),
            request.Pagination.Page,
            request.Pagination.PageSize,
            cancellationToken);
    }
}
