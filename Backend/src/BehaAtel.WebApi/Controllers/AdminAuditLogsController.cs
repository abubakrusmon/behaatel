using BehaAtel.Application.Common.Models;
using BehaAtel.Application.Features.AuditLogs.GetAuditLogs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;

namespace BehaAtel.WebApi.Controllers;

[ApiController]
[Authorize(Policy = "SuperAdminOnly")]
[Route("api/v1/admin/audit-logs")]
public sealed class AdminAuditLogsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortDir = null,
        [FromQuery] Guid? userId = null,
        [FromQuery] string? entityName = null,
        [FromQuery] string? action = null,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        CancellationToken cancellationToken = default) =>
        Ok(await mediator.Send(
            new GetAuditLogsQuery(
                new PaginationQuery(page, pageSize, search, sortBy, sortDir),
                userId,
                entityName,
                action,
                from,
                to),
            cancellationToken));
}
