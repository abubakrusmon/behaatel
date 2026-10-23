using BehaAtel.Application.Common.Models;
using BehaAtel.Application.Features.Announcements.CreateAnnouncement;
using BehaAtel.Application.Features.Announcements.DeleteAnnouncement;
using BehaAtel.Application.Features.Announcements.GetAnnouncements;
using BehaAtel.Application.Features.Announcements.PublishAnnouncement;
using BehaAtel.Application.Features.Announcements.UpdateAnnouncement;
using BehaAtel.Domain.Enums;
using BehaAtel.WebApi.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BehaAtel.WebApi.Controllers;

[ApiController]
[Authorize(Policy = "AdminArea")]
[Route("api/v1/admin/announcements")]
public sealed class AdminAnnouncementsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = nameof(Permission.AnnouncementsManage))]
    public async Task<IActionResult> Get(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortDir = null,
        [FromQuery] bool? isPublished = null,
        CancellationToken cancellationToken = default) =>
        Ok(await mediator.Send(
            new GetAnnouncementsQuery(
                new PaginationQuery(page, pageSize, search, sortBy, sortDir),
                isPublished),
            cancellationToken));

    [HttpPost]
    [Authorize(Policy = nameof(Permission.AnnouncementsManage))]
    public async Task<IActionResult> Create(
        CreateAnnouncementCommand command,
        CancellationToken cancellationToken) =>
        Ok(await mediator.Send(command, cancellationToken));

    [HttpPut("{id:guid}")]
    [Authorize(Policy = nameof(Permission.AnnouncementsManage))]
    public async Task<IActionResult> Update(
        Guid id,
        UpdateAnnouncementRequest request,
        CancellationToken cancellationToken) =>
        Ok(await mediator.Send(new UpdateAnnouncementCommand(id, request.Title, request.Body), cancellationToken));

    [HttpPost("{id:guid}/publish")]
    [Authorize(Policy = nameof(Permission.AnnouncementsManage))]
    public async Task<IActionResult> Publish(Guid id, CancellationToken cancellationToken)
    {
        await mediator.Send(new PublishAnnouncementCommand(id), cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = nameof(Permission.AnnouncementsManage))]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await mediator.Send(new DeleteAnnouncementCommand(id), cancellationToken);
        return NoContent();
    }
}
