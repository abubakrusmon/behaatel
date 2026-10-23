using BehaAtel.Application.Common.Exceptions;
using BehaAtel.Application.Common.Interfaces;
using BehaAtel.Application.Common.Models;
using BehaAtel.Application.Features.Me.Common;
using BehaAtel.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BehaAtel.Application.Features.Me.GetMyCharges;

public sealed class GetMyChargesHandler(IApplicationDbContext dbContext, ICurrentUser currentUser)
    : IRequestHandler<GetMyChargesQuery, PagedList<MyChargeDto>>
{
    public async Task<PagedList<MyChargeDto>> Handle(GetMyChargesQuery request, CancellationToken cancellationToken)
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

        var charges = dbContext.Charges.AsNoTracking()
            .Where(charge => charge.ResidentId == residentId);

        if (request.Period.HasValue)
        {
            var period = new DateOnly(request.Period.Value.Year, request.Period.Value.Month, 1);
            charges = charges.Where(charge => charge.Period == period);
        }

        if (request.Status.HasValue)
        {
            charges = charges.Where(charge => charge.Status == request.Status.Value);
        }

        var sortedCharges = string.Equals(request.Pagination.SortDir, "asc", StringComparison.OrdinalIgnoreCase)
            ? charges.OrderBy(charge => charge.Period)
            : charges.OrderByDescending(charge => charge.Period);

        var query = sortedCharges
            .Select(charge => new
            {
                Charge = charge,
                PaidAmount = dbContext.PaymentAllocations
                    .Where(allocation => allocation.ChargeId == charge.Id)
                    .Sum(allocation => (decimal?)allocation.Amount) ?? 0m
            })
            .Select(item => new MyChargeDto(
                item.Charge.Id,
                item.Charge.Period,
                item.Charge.Type,
                item.Charge.Description,
                item.Charge.Amount,
                item.PaidAmount,
                item.Charge.Amount - item.PaidAmount,
                item.Charge.DueDate,
                item.Charge.Status));

        return await PagedList<MyChargeDto>.CreateAsync(
            query,
            request.Pagination.Page,
            request.Pagination.PageSize,
            cancellationToken);
    }
}
