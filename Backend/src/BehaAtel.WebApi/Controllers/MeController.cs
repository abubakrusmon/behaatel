using BehaAtel.Application.Common.Models;
using BehaAtel.Application.Features.Me.GetMyAnnouncements;
using BehaAtel.Application.Features.Me.GetMyCharges;
using BehaAtel.Application.Features.Me.GetMyMonthlyChart;
using BehaAtel.Application.Features.Me.GetMyPayments;
using BehaAtel.Application.Features.Me.GetMyProfile;
using BehaAtel.Application.Features.Me.GetMyRoom;
using BehaAtel.Application.Features.Me.GetMySummary;
using BehaAtel.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BehaAtel.WebApi.Controllers;

[ApiController]
[Authorize(Policy = "UserOnly")]
[Route("api/v1/me")]
public sealed class MeController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetProfile(CancellationToken cancellationToken) =>
        Ok(await mediator.Send(new GetMyProfileQuery(), cancellationToken));

    [HttpGet("room")]
    public async Task<IActionResult> GetRoom(CancellationToken cancellationToken) =>
        Ok(await mediator.Send(new GetMyRoomQuery(), cancellationToken));

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary(CancellationToken cancellationToken) =>
        Ok(await mediator.Send(new GetMySummaryQuery(), cancellationToken));

    [HttpGet("charges")]
    public async Task<IActionResult> GetCharges(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortDir = null,
        [FromQuery] DateOnly? period = null,
        [FromQuery] ChargeStatus? status = null,
        CancellationToken cancellationToken = default) =>
        Ok(await mediator.Send(
            new GetMyChargesQuery(new PaginationQuery(page, pageSize, search, sortBy, sortDir), period, status),
            cancellationToken));

    [HttpGet("payments")]
    public async Task<IActionResult> GetPayments(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortDir = null,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        CancellationToken cancellationToken = default) =>
        Ok(await mediator.Send(
            new GetMyPaymentsQuery(new PaginationQuery(page, pageSize, search, sortBy, sortDir), from, to),
            cancellationToken));

    [HttpGet("charts/monthly")]
    public async Task<IActionResult> GetMonthlyChart(
        [FromQuery] int months = 12,
        CancellationToken cancellationToken = default) =>
        Ok(await mediator.Send(new GetMyMonthlyChartQuery(months), cancellationToken));

    [HttpGet("announcements")]
    public async Task<IActionResult> GetAnnouncements(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortDir = null,
        CancellationToken cancellationToken = default) =>
        Ok(await mediator.Send(
            new GetMyAnnouncementsQuery(new PaginationQuery(page, pageSize, search, sortBy, sortDir)),
            cancellationToken));
}
