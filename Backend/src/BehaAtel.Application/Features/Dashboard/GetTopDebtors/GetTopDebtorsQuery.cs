using BehaAtel.Application.Features.Dashboard.Common;
using MediatR;

namespace BehaAtel.Application.Features.Dashboard.GetTopDebtors;

public sealed record GetTopDebtorsQuery(int Limit = 10) : IRequest<IReadOnlyCollection<TopDebtorDto>>;
