using BehaAtel.Application.Common.Exceptions;
using BehaAtel.Application.Common.Interfaces;
using BehaAtel.Application.Features.Charges.Common;
using BehaAtel.Domain.Entities;
using BehaAtel.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BehaAtel.Application.Features.Charges.GenerateMonthlyCharges;

public sealed class GenerateMonthlyChargesHandler(
    IApplicationDbContext dbContext,
    IAuditService auditService)
    : IRequestHandler<GenerateMonthlyChargesCommand, GenerateResultDto>
{
    public async Task<GenerateResultDto> Handle(
        GenerateMonthlyChargesCommand request,
        CancellationToken cancellationToken)
    {
        var period = new DateOnly(request.Period.Year, request.Period.Month, 1);
        var billingDay = await GetBillingDayAsync(cancellationToken);
        var dueDay = Math.Min(billingDay, DateTime.DaysInMonth(period.Year, period.Month));
        var dueDate = new DateOnly(period.Year, period.Month, dueDay);

        var occupiedResidents =
            from resident in dbContext.Residents.AsNoTracking()
                .Where(resident => !resident.IsDeleted && resident.Status == ResidentStatus.Active)
            join occupancy in dbContext.Occupancies.AsNoTracking()
                    .Where(occupancy => occupancy.To == null)
                on resident.Id equals occupancy.ResidentId
            join room in dbContext.Rooms.AsNoTracking().Where(room => !room.IsDeleted)
                on occupancy.RoomId equals room.Id
            join building in dbContext.Buildings.AsNoTracking().Where(building => !building.IsDeleted)
                on room.BuildingId equals building.Id
            select new
            {
                ResidentId = resident.Id,
                resident.UserId,
                RoomId = room.Id,
                room.PricePerBed
            };

        var eligibleResidents = await occupiedResidents
            .Distinct()
            .ToListAsync(cancellationToken);

        var existingResidentIds = await dbContext.Charges
            .AsNoTracking()
            .Where(charge => charge.Period == period &&
                             charge.Type == ChargeType.Rent &&
                             charge.Status != ChargeStatus.Cancelled)
            .Select(charge => charge.ResidentId)
            .Distinct()
            .ToListAsync(cancellationToken);
        var existingIds = existingResidentIds.ToHashSet();
        var chargesToCreate = new List<Charge>();
        var skipped = 0;

        foreach (var resident in eligibleResidents)
        {
            if (!existingIds.Add(resident.ResidentId))
            {
                skipped++;
                continue;
            }

            chargesToCreate.Add(Charge.Create(
                resident.ResidentId,
                period,
                ChargeType.Rent,
                resident.PricePerBed,
                dueDate,
                "Ежемесячная оплата проживания"));
        }

        dbContext.Charges.AddRange(chargesToCreate);
        await dbContext.SaveChangesAsync(cancellationToken);

        var result = new GenerateResultDto(chargesToCreate.Count, skipped);
        await auditService.LogAsync("GenerateMonthlyCharges", nameof(Charge), period.ToString("yyyy-MM"),
            null, result, cancellationToken);

        return result;
    }

    private async Task<int> GetBillingDayAsync(CancellationToken cancellationToken)
    {
        var settingValue = await dbContext.Settings
            .AsNoTracking()
            .Where(setting => setting.Key == "Billing.DueDay")
            .Select(setting => setting.Value)
            .SingleOrDefaultAsync(cancellationToken);

        if (settingValue is null)
        {
            return 10;
        }

        if (!int.TryParse(settingValue, out var billingDay) || billingDay is < 1 or > 31)
        {
            throw new ConflictException("Настройка Billing.DueDay должна быть целым числом от 1 до 31.");
        }

        return billingDay;
    }
}
