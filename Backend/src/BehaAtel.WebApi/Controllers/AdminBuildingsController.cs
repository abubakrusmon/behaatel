using BehaAtel.Application.Features.Buildings.CreateBuilding;
using BehaAtel.Application.Features.Buildings.DeleteBuilding;
using BehaAtel.Application.Features.Buildings.GetBuildingById;
using BehaAtel.Application.Features.Buildings.GetBuildings;
using BehaAtel.Application.Features.Buildings.UpdateBuilding;
using BehaAtel.Domain.Enums;
using BehaAtel.WebApi.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BehaAtel.WebApi.Controllers;

[ApiController]
[Authorize(Policy = "AdminArea")]
[Route("api/v1/admin/buildings")]
public sealed class AdminBuildingsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = nameof(Permission.RoomsView))]
    public async Task<IActionResult> Get(CancellationToken cancellationToken) =>
        Ok(await mediator.Send(new GetBuildingsQuery(), cancellationToken));

    [HttpGet("{id:guid}")]
    [Authorize(Policy = nameof(Permission.RoomsView))]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken) =>
        Ok(await mediator.Send(new GetBuildingByIdQuery(id), cancellationToken));

    [HttpPost]
    [Authorize(Policy = nameof(Permission.RoomsManage))]
    public async Task<IActionResult> Create(CreateBuildingCommand command, CancellationToken cancellationToken)
    {
        var building = await mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = building.Id }, building);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = nameof(Permission.RoomsManage))]
    public async Task<IActionResult> Update(
        Guid id,
        UpdateBuildingRequest request,
        CancellationToken cancellationToken) =>
        Ok(await mediator.Send(
            new UpdateBuildingCommand(id, request.Name, request.Address, request.FloorsCount),
            cancellationToken));

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = nameof(Permission.RoomsManage))]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await mediator.Send(new DeleteBuildingCommand(id), cancellationToken);
        return NoContent();
    }
}
