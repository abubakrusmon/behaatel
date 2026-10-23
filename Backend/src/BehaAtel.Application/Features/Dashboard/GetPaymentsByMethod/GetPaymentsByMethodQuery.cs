using BehaAtel.Application.Features.Dashboard.Common;
using MediatR;

namespace BehaAtel.Application.Features.Dashboard.GetPaymentsByMethod;

public sealed record GetPaymentsByMethodQuery(DateTime? From = null, DateTime? To = null)
    : IRequest<IReadOnlyCollection<MethodShareDto>>;
