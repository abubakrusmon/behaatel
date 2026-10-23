using BehaAtel.Application.Common.Models;
using BehaAtel.Application.Features.Me.Common;
using MediatR;

namespace BehaAtel.Application.Features.Me.GetMyPayments;

public sealed record GetMyPaymentsQuery(
    PaginationQuery Pagination,
    DateTime? From = null,
    DateTime? To = null) : IRequest<PagedList<MyPaymentDto>>;
