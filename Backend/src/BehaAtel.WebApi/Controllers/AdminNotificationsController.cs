using BehaAtel.Application.Features.Notifications.CreateNotification;
using BehaAtel.Application.Features.Notifications.GetNotificationRecipients;
using BehaAtel.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BehaAtel.WebApi.Controllers;

[ApiController]
[Authorize(Policy = "AdminArea")]
[Route("api/v1/admin/notifications")]
public sealed class AdminNotificationsController(IMediator mediator) : ControllerBase
{
    [HttpGet("recipients")]
    [Authorize(Policy = nameof(Permission.AnnouncementsManage))]
    public async Task<IActionResult> GetRecipients(CancellationToken cancellationToken) =>
        Ok(await mediator.Send(new GetNotificationRecipientsQuery(), cancellationToken));

    [HttpPost]
    [Authorize(Policy = nameof(Permission.AnnouncementsManage))]
    public async Task<IActionResult> Create(
        CreateNotificationCommand command,
        CancellationToken cancellationToken) =>
        Ok(await mediator.Send(command, cancellationToken));
}
