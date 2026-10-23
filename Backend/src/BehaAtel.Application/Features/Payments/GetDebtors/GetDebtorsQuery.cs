using BehaAtel.Application.Common.Models;
using BehaAtel.Application.Features.Payments.Common;
using MediatR;

namespace BehaAtel.Application.Features.Payments.GetDebtors;

public sealed record GetDebtorsQuery(PaginationQuery Pagination) : IRequest<PagedList<DebtorDto>>;
