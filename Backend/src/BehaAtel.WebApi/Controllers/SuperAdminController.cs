using BehaAtel.Application.Common.Models;
using BehaAtel.Application.Features.Admins.BlockAdmin;
using BehaAtel.Application.Features.Admins.CreateAdmin;
using BehaAtel.Application.Features.Admins.DeleteAdmin;
using BehaAtel.Application.Features.Admins.GetAdminById;
using BehaAtel.Application.Features.Admins.GetAdmins;
using BehaAtel.Application.Features.Admins.ResetAdminPassword;
using BehaAtel.Application.Features.Admins.UnblockAdmin;
using BehaAtel.Application.Features.Admins.UpdateAdminPermissions;
using BehaAtel.WebApi.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BehaAtel.WebApi.Controllers;

[ApiController]
[Authorize(Policy = "SuperAdminOnly")]
[Route("api/v1/superadmin/admins")]
public sealed class SuperAdminController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortDir = null,
        [FromQuery] bool? isActive = null,
        CancellationToken cancellationToken = default) =>
        Ok(await mediator.Send(
            new GetAdminsQuery(new PaginationQuery(page, pageSize, search, sortBy, sortDir), isActive),
            cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken) =>
        Ok(await mediator.Send(new GetAdminByIdQuery(id), cancellationToken));

    [HttpPost]
    public async Task<IActionResult> Create(CreateAdminCommand command, CancellationToken cancellationToken)
    {
        var admin = await mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = admin.Id }, admin);
    }

    [HttpPut("{id:guid}/permissions")]
    public async Task<IActionResult> UpdatePermissions(
        Guid id,
        UpdateAdminPermissionsRequest request,
        CancellationToken cancellationToken)
    {
        await mediator.Send(new UpdateAdminPermissionsCommand(id, request.Permissions), cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/block")]
    public async Task<IActionResult> Block(Guid id, CancellationToken cancellationToken)
    {
        await mediator.Send(new BlockAdminCommand(id), cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/unblock")]
    public async Task<IActionResult> Unblock(Guid id, CancellationToken cancellationToken)
    {
        await mediator.Send(new UnblockAdminCommand(id), cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/reset-password")]
    public async Task<IActionResult> ResetPassword(
        Guid id,
        ResetPasswordRequest request,
        CancellationToken cancellationToken)
    {
        await mediator.Send(new ResetAdminPasswordCommand(id, request.NewPassword), cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await mediator.Send(new DeleteAdminCommand(id), cancellationToken);
        return NoContent();
    }
}
