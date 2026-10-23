using BehaAtel.Application.Common.Models;
using BehaAtel.Application.Features.Payments.Common;
using BehaAtel.Domain.Enums;
using MediatR;

namespace BehaAtel.Application.Features.Payments.GetPayments;

public sealed record GetPaymentsQuery(
    PaginationQuery Pagination,
    Guid? ResidentId = null,
    PaymentStatus? Status = null,
    PaymentMethod? Method = null,
    DateTime? From = null,
    DateTime? To = null,
    decimal? MinAmount = null,
    decimal? MaxAmount = null) : IRequest<PagedList<PaymentDto>>;
