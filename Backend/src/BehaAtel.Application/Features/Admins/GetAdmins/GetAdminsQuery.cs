using BehaAtel.Application.Common.Models;
using BehaAtel.Application.Features.Admins.Common;
using MediatR;

namespace BehaAtel.Application.Features.Admins.GetAdmins;

public sealed record GetAdminsQuery(PaginationQuery Pagination, bool? IsActive) : IRequest<PagedList<AdminDto>>;
