using BehaAtel.Application.Features.Settings.GetSettings;
using BehaAtel.Application.Features.Settings.UpdateSetting;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BehaAtel.WebApi.Controllers;

[ApiController]
[Authorize(Policy = "SuperAdminOnly")]
[Route("api/v1/admin/settings")]
public sealed class AdminSettingsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken) =>
        Ok(await mediator.Send(new GetSettingsQuery(), cancellationToken));

    [HttpPut]
    public async Task<IActionResult> Update(
        UpdateSettingCommand command,
        CancellationToken cancellationToken) =>
        Ok(await mediator.Send(command, cancellationToken));
}
