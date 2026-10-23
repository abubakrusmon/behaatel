using BehaAtel.Application.Common.Interfaces;
using BehaAtel.Application.Common.Models;
using BehaAtel.Application.Features.Admins.Common;
using BehaAtel.Domain.Entities;
using BehaAtel.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BehaAtel.Application.Features.Admins.GetAdmins;

public sealed class GetAdminsHandler(IApplicationDbContext dbContext, ICurrentUser currentUser)
    : IRequestHandler<GetAdminsQuery, PagedList<AdminDto>>
{
    public async Task<PagedList<AdminDto>> Handle(GetAdminsQuery request, CancellationToken cancellationToken)
    {
        AdminAuthorization.RequireSuperAdmin(currentUser);
        var pagination = request.Pagination;
        var query = dbContext.Users
            .AsNoTracking()
            .Include(user => user.Permissions)
            .Where(user => user.Role == Role.Admin && !user.IsDeleted);

        if (request.IsActive.HasValue)
        {
            query = query.Where(user => user.IsActive == request.IsActive.Value);
        }

        if (!string.IsNullOrWhiteSpace(pagination.Search))
        {
            var search = pagination.Search.Trim().ToLower();
            query = query.Where(user => user.Email.ToLower().Contains(search) || user.FullName.ToLower().Contains(search));
        }

        query = ApplySorting(query, pagination.SortBy, pagination.SortDir);
        var totalCount = await query.CountAsync(cancellationToken);
        var admins = await query
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ToListAsync(cancellationToken);

        return PagedList<AdminDto>.CreatePage(admins.Select(AdminDto.FromUser), pagination.Page, pagination.PageSize, totalCount);
    }

    private static IQueryable<User> ApplySorting(IQueryable<User> query, string? sortBy, string? sortDir)
    {
        var descending = string.Equals(sortDir, "desc", StringComparison.OrdinalIgnoreCase);
        return (sortBy?.Trim().ToLower(), descending) switch
        {
            ("fullname", false) => query.OrderBy(user => user.FullName),
            ("fullname", true) => query.OrderByDescending(user => user.FullName),
            ("isactive", false) => query.OrderBy(user => user.IsActive),
            ("isactive", true) => query.OrderByDescending(user => user.IsActive),
            ("createdat", false) => query.OrderBy(user => user.CreatedAt),
            ("createdat", true) => query.OrderByDescending(user => user.CreatedAt),
            ("lastloginat", false) => query.OrderBy(user => user.LastLoginAt),
            ("lastloginat", true) => query.OrderByDescending(user => user.LastLoginAt),
            (_, false) => query.OrderBy(user => user.Email),
            _ => query.OrderByDescending(user => user.Email)
        };
    }
}
