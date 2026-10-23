using BehaAtel.Application.Features.Dashboard.Common;
using MediatR;

namespace BehaAtel.Application.Features.Dashboard.GetRecentPayments;

public sealed record GetRecentPaymentsQuery(int Limit = 10) : IRequest<IReadOnlyCollection<RecentPaymentDto>>;
