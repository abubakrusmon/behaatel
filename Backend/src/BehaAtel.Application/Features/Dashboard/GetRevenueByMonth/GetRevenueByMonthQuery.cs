using BehaAtel.Application.Features.Dashboard.Common;
using MediatR;

namespace BehaAtel.Application.Features.Dashboard.GetRevenueByMonth;

public sealed record GetRevenueByMonthQuery(int Months = 12) : IRequest<IReadOnlyCollection<RevenuePointDto>>;
