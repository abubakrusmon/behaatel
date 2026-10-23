using BehaAtel.Application.Features.Me.Common;
using MediatR;

namespace BehaAtel.Application.Features.Me.GetMyMonthlyChart;

public sealed record GetMyMonthlyChartQuery(int Months = 12) : IRequest<IReadOnlyCollection<MonthlyPointDto>>;
