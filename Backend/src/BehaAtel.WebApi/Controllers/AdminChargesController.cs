using System.Globalization;
using BehaAtel.Application.Common.Exceptions;
using BehaAtel.Application.Common.Models;
using BehaAtel.Application.Features.Charges.CancelCharge;
using BehaAtel.Application.Features.Charges.CreateCharge;
using BehaAtel.Application.Features.Charges.GenerateMonthlyCharges;
using BehaAtel.Application.Features.Charges.GetCharges;
using BehaAtel.Domain.Enums;
using BehaAtel.WebApi.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BehaAtel.WebApi.Controllers;

[ApiController]
[Authorize(Policy = "AdminArea")]
[Route("api/v1/admin/charges")]
public sealed class AdminChargesController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = nameof(Permission.ChargesView))]
    public async Task<IActionResult> Get(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortDir = null,
        [FromQuery] Guid? residentId = null,
        [FromQuery] DateOnly? period = null,
        [FromQuery] ChargeStatus? status = null,
        [FromQuery] ChargeType? type = null,
        CancellationToken cancellationToken = default) =>
        Ok(await mediator.Send(
            new GetChargesQuery(
                new PaginationQuery(page, pageSize, search, sortBy, sortDir),
                residentId,
                period,
                status,
                type),
            cancellationToken));

    [HttpPost]
    [Authorize(Policy = nameof(Permission.ChargesManage))]
    public async Task<IActionResult> Create(CreateChargeCommand command, CancellationToken cancellationToken) =>
        Ok(await mediator.Send(command, cancellationToken));

    [HttpPost("{id:guid}/cancel")]
    [Authorize(Policy = nameof(Permission.ChargesManage))]
    public async Task<IActionResult> Cancel(
        Guid id,
        CancelReasonRequest request,
        CancellationToken cancellationToken)
    {
        await mediator.Send(new CancelChargeCommand(id, request.Reason), cancellationToken);
        return NoContent();
    }

    [HttpPost("generate")]
    [Authorize(Policy = nameof(Permission.ChargesManage))]
    public async Task<IActionResult> Generate(
        [FromQuery] string period,
        CancellationToken cancellationToken)
    {
        if (!DateOnly.TryParseExact(
                $"{period}-01",
                "yyyy-MM-dd",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var month))
        {
            throw new ValidationException(new Dictionary<string, string[]>
            {
                [nameof(period)] = ["Период должен быть в формате yyyy-MM."]
            });
        }

        return Ok(await mediator.Send(new GenerateMonthlyChargesCommand(month), cancellationToken));
    }
}
