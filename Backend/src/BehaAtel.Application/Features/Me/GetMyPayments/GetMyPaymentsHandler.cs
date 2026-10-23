using BehaAtel.Application.Common.Exceptions;
using BehaAtel.Application.Common.Interfaces;
using BehaAtel.Application.Common.Models;
using BehaAtel.Application.Features.Me.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BehaAtel.Application.Features.Me.GetMyPayments;

public sealed class GetMyPaymentsHandler(IApplicationDbContext dbContext, ICurrentUser currentUser)
    : IRequestHandler<GetMyPaymentsQuery, PagedList<MyPaymentDto>>
{
    public async Task<PagedList<MyPaymentDto>> Handle(GetMyPaymentsQuery request, CancellationToken cancellationToken)
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

        var payments = dbContext.Payments.AsNoTracking()
            .Where(payment => payment.ResidentId == residentId);

        if (request.From.HasValue)
        {
            payments = payments.Where(payment => payment.PaidAt >= request.From.Value);
        }

        if (request.To.HasValue)
        {
            payments = payments.Where(payment => payment.PaidAt <= request.To.Value);
        }

        var query = payments.OrderByDescending(payment => payment.PaidAt)
            .Select(payment => new MyPaymentDto(
                payment.Id,
                payment.Amount,
                payment.Method,
                payment.PaidAt,
                payment.ReceiptNumber,
                payment.Status));
        return await PagedList<MyPaymentDto>.CreateAsync(
            query, request.Pagination.Page, request.Pagination.PageSize, cancellationToken);
    }
}
