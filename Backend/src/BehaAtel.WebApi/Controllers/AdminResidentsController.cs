using BehaAtel.Application.Common.Models;
using BehaAtel.Application.Features.Residents.BlockResident;
using BehaAtel.Application.Features.Residents.CheckInResident;
using BehaAtel.Application.Features.Residents.CheckOutResident;
using BehaAtel.Application.Features.Residents.CreateResident;
using BehaAtel.Application.Features.Residents.DeleteResident;
using BehaAtel.Application.Features.Residents.GetResidentById;
using BehaAtel.Application.Features.Residents.GetResidents;
using BehaAtel.Application.Features.Residents.ResetResidentPassword;
using BehaAtel.Application.Features.Residents.UnblockResident;
using BehaAtel.Application.Features.Residents.UpdateResident;
using BehaAtel.Domain.Enums;
using BehaAtel.WebApi.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BehaAtel.WebApi.Controllers;

[ApiController]
[Authorize(Policy = "AdminArea")]
[Route("api/v1/admin/residents")]
public sealed class AdminResidentsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = nameof(Permission.ResidentsView))]
    public async Task<IActionResult> Get(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortDir = null,
        [FromQuery] ResidentStatus? status = null,
        [FromQuery] Guid? buildingId = null,
        [FromQuery] Guid? roomId = null,
        [FromQuery] bool? hasDebt = null,
        CancellationToken cancellationToken = default) =>
        Ok(await mediator.Send(
            new GetResidentsQuery(
                new PaginationQuery(page, pageSize, search, sortBy, sortDir),
                status,
                buildingId,
                roomId,
                hasDebt),
            cancellationToken));

    [HttpGet("{id:guid}")]
    [Authorize(Policy = nameof(Permission.ResidentsView))]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken) =>
        Ok(await mediator.Send(new GetResidentByIdQuery(id), cancellationToken));

    [HttpPost]
    [Authorize(Policy = nameof(Permission.ResidentsManage))]
    public async Task<IActionResult> Create(CreateResidentCommand command, CancellationToken cancellationToken)
    {
        var resident = await mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = resident.Id }, resident);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = nameof(Permission.ResidentsManage))]
    public async Task<IActionResult> Update(
        Guid id,
        UpdateResidentRequest request,
        CancellationToken cancellationToken)
    {
        await mediator.Send(
            new UpdateResidentCommand(id, request.FullName, request.Phone, request.PassportNumber, request.BirthDate, request.Organization),
            cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = nameof(Permission.ResidentsManage))]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await mediator.Send(new DeleteResidentCommand(id), cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/check-in")]
    [Authorize(Policy = nameof(Permission.ResidentsManage))]
    public async Task<IActionResult> CheckIn(
        Guid id,
        CheckInResidentRequest request,
        CancellationToken cancellationToken)
    {
        await mediator.Send(new CheckInResidentCommand(id, request.RoomId, request.Date), cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/check-out")]
    [Authorize(Policy = nameof(Permission.ResidentsManage))]
    public async Task<IActionResult> CheckOut(
        Guid id,
        CheckOutResidentRequest request,
        CancellationToken cancellationToken)
    {
        await mediator.Send(new CheckOutResidentCommand(id, request.Date), cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/block")]
    [Authorize(Policy = nameof(Permission.ResidentsManage))]
    public async Task<IActionResult> Block(Guid id, CancellationToken cancellationToken)
    {
        await mediator.Send(new BlockResidentCommand(id), cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/unblock")]
    [Authorize(Policy = nameof(Permission.ResidentsManage))]
    public async Task<IActionResult> Unblock(Guid id, CancellationToken cancellationToken)
    {
        await mediator.Send(new UnblockResidentCommand(id), cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/reset-password")]
    [Authorize(Policy = nameof(Permission.ResidentsManage))]
    public async Task<IActionResult> ResetPassword(
        Guid id,
        ResetPasswordRequest request,
        CancellationToken cancellationToken)
    {
        await mediator.Send(new ResetResidentPasswordCommand(id, request.NewPassword), cancellationToken);
        return NoContent();
    }
}
