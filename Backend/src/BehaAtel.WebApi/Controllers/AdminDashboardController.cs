using BehaAtel.Application.Features.Dashboard.GetDashboardSummary;
using BehaAtel.Application.Features.Dashboard.GetOccupancyByBuilding;
using BehaAtel.Application.Features.Dashboard.GetPaymentsByMethod;
using BehaAtel.Application.Features.Dashboard.GetRecentPayments;
using BehaAtel.Application.Features.Dashboard.GetRevenueByMonth;
using BehaAtel.Application.Features.Dashboard.GetTopDebtors;
using BehaAtel.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BehaAtel.WebApi.Controllers;

[ApiController]
[Authorize(Policy = "AdminArea")]
[Route("api/v1/admin/dashboard")]
public sealed class AdminDashboardController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = nameof(Permission.ReportsView))]
    public async Task<IActionResult> GetSummary(CancellationToken cancellationToken) =>
        Ok(await mediator.Send(new GetDashboardSummaryQuery(), cancellationToken));

    [HttpGet("charts/revenue")]
    [Authorize(Policy = nameof(Permission.ReportsView))]
    public async Task<IActionResult> GetRevenue(
        [FromQuery] int months = 12,
        CancellationToken cancellationToken = default) =>
        Ok(await mediator.Send(new GetRevenueByMonthQuery(months), cancellationToken));

    [HttpGet("charts/payment-methods")]
    [Authorize(Policy = nameof(Permission.ReportsView))]
    public async Task<IActionResult> GetPaymentsByMethod(
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        CancellationToken cancellationToken = default) =>
        Ok(await mediator.Send(new GetPaymentsByMethodQuery(from, to), cancellationToken));

    [HttpGet("charts/occupancy")]
    [Authorize(Policy = nameof(Permission.ReportsView))]
    public async Task<IActionResult> GetOccupancy(CancellationToken cancellationToken) =>
        Ok(await mediator.Send(new GetOccupancyByBuildingQuery(), cancellationToken));

    [HttpGet("top-debtors")]
    [Authorize(Policy = nameof(Permission.ReportsView))]
    public async Task<IActionResult> GetTopDebtors(
        [FromQuery] int limit = 10,
        CancellationToken cancellationToken = default) =>
        Ok(await mediator.Send(new GetTopDebtorsQuery(limit), cancellationToken));

    [HttpGet("recent-payments")]
    [Authorize(Policy = nameof(Permission.ReportsView))]
    public async Task<IActionResult> GetRecentPayments(
        [FromQuery] int limit = 10,
        CancellationToken cancellationToken = default) =>
        Ok(await mediator.Send(new GetRecentPaymentsQuery(limit), cancellationToken));
}
