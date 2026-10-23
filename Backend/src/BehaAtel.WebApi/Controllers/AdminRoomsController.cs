using BehaAtel.Application.Common.Models;
using BehaAtel.Application.Features.Rooms.CreateRoom;
using BehaAtel.Application.Features.Rooms.DeleteRoom;
using BehaAtel.Application.Features.Rooms.GetRoomById;
using BehaAtel.Application.Features.Rooms.GetRooms;
using BehaAtel.Application.Features.Rooms.UpdateRoom;
using BehaAtel.Domain.Enums;
using BehaAtel.WebApi.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BehaAtel.WebApi.Controllers;

[ApiController]
[Authorize(Policy = "AdminArea")]
[Route("api/v1/admin/rooms")]
public sealed class AdminRoomsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = nameof(Permission.RoomsView))]
    public async Task<IActionResult> Get(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortDir = null,
        [FromQuery] Guid? buildingId = null,
        [FromQuery] int? floor = null,
        [FromQuery] RoomType? type = null,
        [FromQuery] bool? onlyWithFreeBeds = null,
        CancellationToken cancellationToken = default) =>
        Ok(await mediator.Send(
            new GetRoomsQuery(
                new PaginationQuery(page, pageSize, search, sortBy, sortDir),
                buildingId,
                floor,
                type,
                onlyWithFreeBeds),
            cancellationToken));

    [HttpGet("{id:guid}")]
    [Authorize(Policy = nameof(Permission.RoomsView))]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken) =>
        Ok(await mediator.Send(new GetRoomByIdQuery(id), cancellationToken));

    [HttpPost]
    [Authorize(Policy = nameof(Permission.RoomsManage))]
    public async Task<IActionResult> Create(CreateRoomCommand command, CancellationToken cancellationToken)
    {
        var room = await mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = room.Id }, room);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = nameof(Permission.RoomsManage))]
    public async Task<IActionResult> Update(Guid id, UpdateRoomRequest request, CancellationToken cancellationToken) =>
        Ok(await mediator.Send(
            new UpdateRoomCommand(id, request.Number, request.Floor, request.Capacity, request.PricePerBed, request.Type),
            cancellationToken));

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = nameof(Permission.RoomsManage))]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await mediator.Send(new DeleteRoomCommand(id), cancellationToken);
        return NoContent();
    }
}
