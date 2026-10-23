using BehaAtel.Application.Common.Models;
using BehaAtel.Application.Features.AuditLogs.Common;
using MediatR;

namespace BehaAtel.Application.Features.AuditLogs.GetAuditLogs;

public sealed record GetAuditLogsQuery(
    PaginationQuery Pagination,
    Guid? UserId = null,
    string? EntityName = null,
    string? Action = null,
    DateTime? From = null,
    DateTime? To = null) : IRequest<PagedList<AuditLogDto>>;
