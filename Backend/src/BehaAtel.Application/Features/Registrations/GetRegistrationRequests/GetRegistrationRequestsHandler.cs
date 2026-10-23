using BehaAtel.Application.Common.Interfaces;
using BehaAtel.Application.Common.Models;
using BehaAtel.Application.Features.Registrations.Common;
using BehaAtel.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BehaAtel.Application.Features.Registrations.GetRegistrationRequests;

public sealed class GetRegistrationRequestsHandler(IApplicationDbContext db)
    : IRequestHandler<GetRegistrationRequestsQuery, PagedList<RegistrationRequestDto>>
{
    public Task<PagedList<RegistrationRequestDto>> Handle(
        GetRegistrationRequestsQuery request, CancellationToken cancellationToken)
    {
        var query = db.Users.AsNoTracking().Where(user => user.Role == Role.User);
        var status = request.Status ?? ApprovalStatus.Pending;
        query = query.Where(user => user.ApprovalStatus == status);
        if (!string.IsNullOrWhiteSpace(request.Pagination.Search))
        {
            var search = request.Pagination.Search.Trim().ToLower();
            query = query.Where(user => user.Email.ToLower().Contains(search) || user.FullName.ToLower().Contains(search));
        }
        var results = query.OrderByDescending(user => user.RegistrationRequestedAt)
            .Select(user => new RegistrationRequestDto(user.Id, user.FullName, user.Email, user.Phone,
                user.RegistrationRequestedAt, user.ApprovalStatus, user.EmailConfirmed));
        return PagedList<RegistrationRequestDto>.CreateAsync(results, request.Pagination.Page,
            request.Pagination.PageSize, cancellationToken);
    }
}
