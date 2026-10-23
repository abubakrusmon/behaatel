using BehaAtel.Application.Common.Models;
using BehaAtel.Application.Features.Charges.Common;
using BehaAtel.Domain.Enums;
using MediatR;

namespace BehaAtel.Application.Features.Charges.GetCharges;

public sealed record GetChargesQuery(
    PaginationQuery Pagination,
    Guid? ResidentId = null,
    DateOnly? Period = null,
    ChargeStatus? Status = null,
    ChargeType? Type = null) : IRequest<PagedList<ChargeDto>>;
