using BehaAtel.Application.Features.Me.Common;
using MediatR;

namespace BehaAtel.Application.Features.Me.GetMySummary;

public sealed record GetMySummaryQuery : IRequest<MySummaryDto>;
