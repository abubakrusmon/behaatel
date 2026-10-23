using BehaAtel.Application.Common.Interfaces;
using BehaAtel.Application.Features.Dashboard.Common;
using BehaAtel.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BehaAtel.Application.Features.Dashboard.GetRecentPayments;

public sealed class GetRecentPaymentsHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetRecentPaymentsQuery, IReadOnlyCollection<RecentPaymentDto>>
{
    public async Task<IReadOnlyCollection<RecentPaymentDto>> Handle(
        GetRecentPaymentsQuery request,
        CancellationToken cancellationToken) =>
        await (
            from payment in dbContext.Payments.AsNoTracking()
                .Where(payment => payment.Status == PaymentStatus.Completed)
            join resident in dbContext.Residents.AsNoTracking().Where(resident => !resident.IsDeleted)
                on payment.ResidentId equals resident.Id
            join user in dbContext.Users.AsNoTracking().Where(user => !user.IsDeleted)
                on resident.UserId equals user.Id
            orderby payment.PaidAt descending
            select new RecentPaymentDto(
                payment.Id,
                user.FullName,
                payment.Amount,
                payment.Method,
                payment.PaidAt,
                payment.ReceiptNumber))
            .Take(request.Limit)
            .ToListAsync(cancellationToken);
}
