using BehaAtel.Application.Common.Exceptions;
using BehaAtel.Application.Common.Interfaces;
using BehaAtel.Application.Features.Residents.Common;
using BehaAtel.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BehaAtel.Application.Features.Residents.GetResidentById;

public sealed class GetResidentByIdHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetResidentByIdQuery, ResidentDetailsDto>
{
    public async Task<ResidentDetailsDto> Handle(GetResidentByIdQuery request, CancellationToken cancellationToken)
    {
        var payments = dbContext.Payments.AsNoTracking();
        var charges = dbContext.Charges.AsNoTracking();
        var resident = await (
            from candidate in dbContext.Residents.AsNoTracking()
                .Where(candidate => candidate.Id == request.Id && !candidate.IsDeleted)
            join user in dbContext.Users.AsNoTracking()
                    .Where(candidate => !candidate.IsDeleted && candidate.Role == Role.User)
                on candidate.UserId equals user.Id
            select new
            {
                candidate.Id,
                candidate.UserId,
                user.FullName,
                user.Email,
                user.Phone,
                candidate.PassportNumber,
                candidate.BirthDate,
                candidate.Organization,
                candidate.Status,
                user.IsActive,
                candidate.CheckInDate,
                candidate.CheckOutDate,
                Balance = (payments
                    .Where(payment => payment.ResidentId == candidate.Id && payment.Status == PaymentStatus.Completed)
                    .Sum(payment => (decimal?)payment.Amount) ?? 0m)
                    - (charges
                        .Where(charge => charge.ResidentId == candidate.Id && charge.Status != ChargeStatus.Cancelled)
                        .Sum(charge => (decimal?)charge.Amount) ?? 0m),
                TotalCharged = charges
                    .Where(charge => charge.ResidentId == candidate.Id && charge.Status != ChargeStatus.Cancelled)
                    .Sum(charge => (decimal?)charge.Amount) ?? 0m,
                TotalPaid = payments
                    .Where(payment => payment.ResidentId == candidate.Id && payment.Status == PaymentStatus.Completed)
                    .Sum(payment => (decimal?)payment.Amount) ?? 0m,
                CurrentRoom = (
                    from occupancy in dbContext.Occupancies.AsNoTracking()
                        .Where(occupancy => occupancy.ResidentId == candidate.Id && occupancy.To == null)
                    join room in dbContext.Rooms.AsNoTracking().Where(room => !room.IsDeleted)
                        on occupancy.RoomId equals room.Id
                    join building in dbContext.Buildings.AsNoTracking().Where(building => !building.IsDeleted)
                        on room.BuildingId equals building.Id
                    select new CurrentRoomDto(building.Name, room.Number, room.Floor, room.PricePerBed))
                    .FirstOrDefault()
            })
            .SingleOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException("Resident", request.Id);

        var occupancyHistory = await (
            from occupancy in dbContext.Occupancies.AsNoTracking()
                .Where(occupancy => occupancy.ResidentId == request.Id)
            join room in dbContext.Rooms.AsNoTracking().Where(room => !room.IsDeleted)
                on occupancy.RoomId equals room.Id
            join building in dbContext.Buildings.AsNoTracking().Where(building => !building.IsDeleted)
                on room.BuildingId equals building.Id
            orderby occupancy.From descending, occupancy.To descending
            select new OccupancyHistoryDto(occupancy.From, occupancy.To, room.Number, building.Name))
            .ToListAsync(cancellationToken);

        return new ResidentDetailsDto(
            resident.Id,
            resident.UserId,
            resident.FullName,
            resident.Email,
            resident.Phone,
            resident.PassportNumber,
            resident.BirthDate,
            resident.Organization,
            resident.Status,
            resident.IsActive,
            resident.CheckInDate,
            resident.CheckOutDate,
            resident.CurrentRoom,
            resident.Balance,
            resident.TotalCharged,
            resident.TotalPaid,
            occupancyHistory);
    }
}
