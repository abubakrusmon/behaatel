using BehaAtel.Application.Common.Interfaces;
using BehaAtel.Application.Common.Models;
using BehaAtel.Application.Features.Payments.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BehaAtel.Application.Features.Payments.GetPayments;

public sealed class GetPaymentsHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetPaymentsQuery, PagedList<PaymentDto>>
{
    public async Task<PagedList<PaymentDto>> Handle(GetPaymentsQuery request, CancellationToken cancellationToken)
    {
        var payments = dbContext.Payments.AsNoTracking();

        if (request.ResidentId.HasValue)
        {
            payments = payments.Where(payment => payment.ResidentId == request.ResidentId.Value);
        }

        if (request.Status.HasValue)
        {
            payments = payments.Where(payment => payment.Status == request.Status.Value);
        }

        if (request.Method.HasValue)
        {
            payments = payments.Where(payment => payment.Method == request.Method.Value);
        }

        if (request.From.HasValue)
        {
            payments = payments.Where(payment => payment.PaidAt >= request.From.Value);
        }

        if (request.To.HasValue)
        {
            payments = payments.Where(payment => payment.PaidAt <= request.To.Value);
        }

        if (request.MinAmount.HasValue)
        {
            payments = payments.Where(payment => payment.Amount >= request.MinAmount.Value);
        }

        if (request.MaxAmount.HasValue)
        {
            payments = payments.Where(payment => payment.Amount <= request.MaxAmount.Value);
        }

        var query =
            from payment in payments
            join resident in dbContext.Residents.AsNoTracking().Where(resident => !resident.IsDeleted)
                on payment.ResidentId equals resident.Id
            join user in dbContext.Users.AsNoTracking().Where(user => !user.IsDeleted)
                on resident.UserId equals user.Id
            select new
            {
                Payment = payment,
                ResidentName = user.FullName,
                AllocatedAmount = dbContext.PaymentAllocations
                    .Where(allocation => allocation.PaymentId == payment.Id)
                    .Sum(allocation => (decimal?)allocation.Amount) ?? 0m
            };

        if (!string.IsNullOrWhiteSpace(request.Pagination.Search))
        {
            var search = request.Pagination.Search.Trim().ToLower();
            query = query.Where(result =>
                result.ResidentName.ToLower().Contains(search) ||
                result.Payment.ReceiptNumber.ToLower().Contains(search));
        }

        var descending = string.Equals(request.Pagination.SortDir, "desc", StringComparison.OrdinalIgnoreCase);
        query = (request.Pagination.SortBy?.Trim().ToLower(), descending) switch
        {
            ("amount", false) => query.OrderBy(result => result.Payment.Amount),
            ("amount", true) => query.OrderByDescending(result => result.Payment.Amount),
            ("paidat", false) => query.OrderBy(result => result.Payment.PaidAt),
            (_, _) => query.OrderByDescending(result => result.Payment.PaidAt)
        };

        return await PagedList<PaymentDto>.CreateAsync(
            query.Select(result => new PaymentDto(
                result.Payment.Id,
                result.Payment.ResidentId,
                result.ResidentName,
                result.Payment.Amount,
                result.Payment.Method,
                result.Payment.Status,
                result.Payment.PaidAt,
                result.Payment.ReceiptNumber,
                result.Payment.Comment,
                result.AllocatedAmount,
                result.Payment.Amount - result.AllocatedAmount)),
            request.Pagination.Page,
            request.Pagination.PageSize,
            cancellationToken);
    }
}
