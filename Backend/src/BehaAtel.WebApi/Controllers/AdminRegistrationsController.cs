using BehaAtel.Application.Common.Models;
using BehaAtel.Application.Features.Registrations.ApproveRegistration;
using BehaAtel.Application.Features.Registrations.GetRegistrationRequests;
using BehaAtel.Application.Features.Registrations.RejectRegistration;
using BehaAtel.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BehaAtel.WebApi.Controllers;

[ApiController]
[Authorize(Policy = "AdminArea")]
[Route("api/v1/admin/registrations")]
public sealed class AdminRegistrationsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = nameof(Permission.ResidentsView))]
    public async Task<IActionResult> Get(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortDir = null,
        [FromQuery] ApprovalStatus? status = null,
        CancellationToken cancellationToken = default) =>
        Ok(await mediator.Send(new GetRegistrationRequestsQuery(
            new PaginationQuery(page, pageSize, search, sortBy, sortDir), status), cancellationToken));

    [HttpPost("{userId:guid}/approve")]
    [Authorize(Policy = nameof(Permission.ResidentsManage))]
    public async Task<IActionResult> Approve(Guid userId, CancellationToken cancellationToken)
    {
        await mediator.Send(new ApproveRegistrationCommand(userId), cancellationToken);
        return NoContent();
    }

    [HttpPost("{userId:guid}/reject")]
    [Authorize(Policy = nameof(Permission.ResidentsManage))]
    public async Task<IActionResult> Reject(Guid userId, RejectRegistrationRequest request, CancellationToken cancellationToken)
    {
        await mediator.Send(new RejectRegistrationCommand(userId, request.Reason), cancellationToken);
        return NoContent();
    }

    public sealed record RejectRegistrationRequest(string? Reason);
}
