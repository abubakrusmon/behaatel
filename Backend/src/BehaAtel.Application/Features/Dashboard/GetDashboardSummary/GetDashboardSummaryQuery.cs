using BehaAtel.Application.Features.Dashboard.Common;
using MediatR;

namespace BehaAtel.Application.Features.Dashboard.GetDashboardSummary;

public sealed record GetDashboardSummaryQuery : IRequest<DashboardSummaryDto>;
