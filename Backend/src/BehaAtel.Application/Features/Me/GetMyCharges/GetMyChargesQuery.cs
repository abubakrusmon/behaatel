using BehaAtel.Application.Common.Models;
using BehaAtel.Application.Features.Me.Common;
using BehaAtel.Domain.Enums;
using MediatR;

namespace BehaAtel.Application.Features.Me.GetMyCharges;

public sealed record GetMyChargesQuery(
    PaginationQuery Pagination,
    DateOnly? Period = null,
    ChargeStatus? Status = null) : IRequest<PagedList<MyChargeDto>>;
