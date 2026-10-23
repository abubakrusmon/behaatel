using BehaAtel.Application.Common.Interfaces;
using BehaAtel.Application.Common.Models;
using BehaAtel.Application.Features.Charges.Common;
using BehaAtel.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BehaAtel.Application.Features.Charges.GetCharges;

public sealed class GetChargesHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetChargesQuery, PagedList<ChargeDto>>
{
    public async Task<PagedList<ChargeDto>> Handle(GetChargesQuery request, CancellationToken cancellationToken)
    {
        var charges = dbContext.Charges.AsNoTracking();

        if (request.ResidentId.HasValue)
        {
            charges = charges.Where(charge => charge.ResidentId == request.ResidentId.Value);
        }

        if (request.Period.HasValue)
        {
            var period = new DateOnly(request.Period.Value.Year, request.Period.Value.Month, 1);
            charges = charges.Where(charge => charge.Period == period);
        }

        if (request.Status.HasValue)
        {
            charges = charges.Where(charge => charge.Status == request.Status.Value);
        }

        if (request.Type.HasValue)
        {
            charges = charges.Where(charge => charge.Type == request.Type.Value);
        }

        var query =
            from charge in charges
            join resident in dbContext.Residents.AsNoTracking().Where(resident => !resident.IsDeleted)
                on charge.ResidentId equals resident.Id
            join user in dbContext.Users.AsNoTracking().Where(user => !user.IsDeleted)
                on resident.UserId equals user.Id
            let paidAmount = dbContext.PaymentAllocations
                .Where(allocation => allocation.ChargeId == charge.Id)
                .Sum(allocation => (decimal?)allocation.Amount) ?? 0m
            select new
            {
                Charge = charge,
                ResidentName = user.FullName,
                PaidAmount = paidAmount
            };

        var descending = !string.Equals(request.Pagination.SortDir, "asc", StringComparison.OrdinalIgnoreCase);
        query = (request.Pagination.SortBy?.Trim().ToLower(), descending) switch
        {
            ("amount", false) => query.OrderBy(item => item.Charge.Amount),
            ("amount", true) => query.OrderByDescending(item => item.Charge.Amount),
            ("duedate", false) => query.OrderBy(item => item.Charge.DueDate),
            ("duedate", true) => query.OrderByDescending(item => item.Charge.DueDate),
            ("status", false) => query.OrderBy(item => item.Charge.Status),
            ("status", true) => query.OrderByDescending(item => item.Charge.Status),
            ("residentname", false) => query.OrderBy(item => item.ResidentName),
            ("residentname", true) => query.OrderByDescending(item => item.ResidentName),
            (_, false) => query.OrderBy(item => item.Charge.Period).ThenBy(item => item.ResidentName),
            _ => query.OrderByDescending(item => item.Charge.Period).ThenBy(item => item.ResidentName)
        };

        return await PagedList<ChargeDto>.CreateAsync(
            query.Select(item => new ChargeDto(
                item.Charge.Id,
                item.Charge.ResidentId,
                item.ResidentName,
                item.Charge.Period,
                item.Charge.Type,
                item.Charge.Amount,
                item.PaidAmount,
                item.Charge.Amount - item.PaidAmount,
                item.Charge.DueDate,
                item.Charge.Status)),
            request.Pagination.Page,
            request.Pagination.PageSize,
            cancellationToken);
    }
}
