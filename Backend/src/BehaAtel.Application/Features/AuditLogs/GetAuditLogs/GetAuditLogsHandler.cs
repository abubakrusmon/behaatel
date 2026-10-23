using BehaAtel.Application.Common.Interfaces;
using BehaAtel.Application.Common.Models;
using BehaAtel.Application.Features.AuditLogs.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BehaAtel.Application.Features.AuditLogs.GetAuditLogs;

public sealed class GetAuditLogsHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetAuditLogsQuery, PagedList<AuditLogDto>>
{
    public async Task<PagedList<AuditLogDto>> Handle(
        GetAuditLogsQuery request,
        CancellationToken cancellationToken)
    {
        var auditLogs = dbContext.AuditLogs.AsNoTracking();

        if (request.UserId.HasValue)
        {
            auditLogs = auditLogs.Where(log => log.UserId == request.UserId.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.EntityName))
        {
            var entityName = request.EntityName.Trim();
            auditLogs = auditLogs.Where(log => log.EntityName == entityName);
        }

        if (!string.IsNullOrWhiteSpace(request.Action))
        {
            var action = request.Action.Trim();
            auditLogs = auditLogs.Where(log => log.Action == action);
        }

        if (request.From.HasValue)
        {
            auditLogs = auditLogs.Where(log => log.CreatedAt >= request.From.Value);
        }

        if (request.To.HasValue)
        {
            auditLogs = auditLogs.Where(log => log.CreatedAt <= request.To.Value);
        }

        auditLogs = auditLogs.OrderByDescending(log => log.CreatedAt);

        var query =
            from auditLog in auditLogs
            join user in dbContext.Users.AsNoTracking().Where(user => !user.IsDeleted)
                on auditLog.UserId equals (Guid?)user.Id into users
            from user in users.DefaultIfEmpty()
            select new AuditLogDto(
                auditLog.Id,
                auditLog.UserId,
                user == null ? null : user.FullName,
                auditLog.Action,
                auditLog.EntityName,
                auditLog.EntityId,
                auditLog.OldValues,
                auditLog.NewValues,
                auditLog.IpAddress,
                auditLog.CreatedAt);
        return await PagedList<AuditLogDto>.CreateAsync(
            query,
            request.Pagination.Page,
            request.Pagination.PageSize,
            cancellationToken);
    }
}
