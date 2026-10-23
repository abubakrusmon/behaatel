using BehaAtel.Application.Common.Models;
using BehaAtel.Application.Features.Registrations.Common;
using BehaAtel.Domain.Enums;
using MediatR;

namespace BehaAtel.Application.Features.Registrations.GetRegistrationRequests;

public sealed record GetRegistrationRequestsQuery(
    PaginationQuery Pagination,
    ApprovalStatus? Status) : IRequest<PagedList<RegistrationRequestDto>>;
