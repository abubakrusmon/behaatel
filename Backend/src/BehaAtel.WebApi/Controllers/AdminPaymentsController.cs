using System.Globalization;
using System.Text;
using BehaAtel.Application.Common.Models;
using BehaAtel.Application.Features.Payments.CancelPayment;
using BehaAtel.Application.Features.Payments.Common;
using BehaAtel.Application.Features.Payments.CreatePayment;
using BehaAtel.Application.Features.Payments.GetDebtors;
using BehaAtel.Application.Features.Payments.GetPayments;
using BehaAtel.Domain.Enums;
using BehaAtel.WebApi.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BehaAtel.WebApi.Controllers;

[ApiController]
[Authorize(Policy = "AdminArea")]
[Route("api/v1/admin")]
public sealed class AdminPaymentsController(IMediator mediator) : ControllerBase
{
    [HttpGet("payments")]
    [Authorize(Policy = nameof(Permission.PaymentsView))]
    public async Task<IActionResult> GetPayments(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortDir = null,
        [FromQuery] Guid? residentId = null,
        [FromQuery] PaymentStatus? status = null,
        [FromQuery] PaymentMethod? method = null,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        [FromQuery] decimal? minAmount = null,
        [FromQuery] decimal? maxAmount = null,
        CancellationToken cancellationToken = default) =>
        Ok(await mediator.Send(
            BuildQuery(page, pageSize, search, sortBy, sortDir, residentId, status, method, from, to, minAmount, maxAmount),
            cancellationToken));

    [HttpPost("payments")]
    [Authorize(Policy = nameof(Permission.PaymentsManage))]
    public async Task<IActionResult> CreatePayment(
        CreatePaymentCommand command,
        CancellationToken cancellationToken) =>
        Ok(await mediator.Send(command, cancellationToken));

    [HttpPost("payments/{id:guid}/cancel")]
    [Authorize(Policy = nameof(Permission.PaymentsManage))]
    public async Task<IActionResult> CancelPayment(
        Guid id,
        CancelReasonRequest request,
        CancellationToken cancellationToken)
    {
        await mediator.Send(new CancelPaymentCommand(id, request.Reason), cancellationToken);
        return NoContent();
    }

    [HttpGet("debtors")]
    [Authorize(Policy = nameof(Permission.PaymentsView))]
    public async Task<IActionResult> GetDebtors(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortDir = null,
        CancellationToken cancellationToken = default) =>
        Ok(await mediator.Send(
            new GetDebtorsQuery(new PaginationQuery(page, pageSize, search, sortBy, sortDir)),
            cancellationToken));

    [HttpGet("payments/export")]
    [HttpGet("/api/v1/admin/transactions/export")]
    [Authorize(Policy = nameof(Permission.PaymentsView))]
    public async Task<IActionResult> Export(
        [FromQuery] string? search = null,
        [FromQuery] Guid? residentId = null,
        [FromQuery] PaymentStatus? status = null,
        [FromQuery] PaymentMethod? method = null,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        [FromQuery] decimal? minAmount = null,
        [FromQuery] decimal? maxAmount = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortDir = null,
        CancellationToken cancellationToken = default)
    {
        var query = BuildQuery(
            1, 100, search, sortBy, sortDir, residentId, status, method, from, to, minAmount, maxAmount);
        var payments = new List<PaymentDto>(capacity: 10_000);
        var page = 1;

        while (payments.Count < 10_000)
        {
            var paged = await mediator.Send(
                query with
                {
                    Pagination = new PaginationQuery(page, 100, search, sortBy, sortDir)
                },
                cancellationToken);
            payments.AddRange(paged.Items.Take(10_000 - payments.Count));
            if (!paged.HasNext || paged.Items.Count == 0)
            {
                break;
            }

            page++;
        }

        var csv = new StringBuilder();
        csv.AppendLine("Id;ResidentId;ResidentName;Amount;Method;Status;PaidAt;ReceiptNumber;Comment;AllocatedAmount;UnallocatedAmount");
        foreach (var payment in payments)
        {
            csv.Append(Csv(payment.Id.ToString()))
                .Append(';').Append(Csv(payment.ResidentId.ToString()))
                .Append(';').Append(Csv(payment.ResidentName))
                .Append(';').Append(payment.Amount.ToString(CultureInfo.InvariantCulture))
                .Append(';').Append(Csv(payment.Method.ToString()))
                .Append(';').Append(Csv(payment.Status.ToString()))
                .Append(';').Append(payment.PaidAt.ToString("O", CultureInfo.InvariantCulture))
                .Append(';').Append(Csv(payment.ReceiptNumber))
                .Append(';').Append(Csv(payment.Comment ?? string.Empty))
                .Append(';').Append(payment.AllocatedAmount.ToString(CultureInfo.InvariantCulture))
                .Append(';').Append(payment.UnallocatedAmount.ToString(CultureInfo.InvariantCulture))
                .AppendLine();
        }

        var encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: true);
        var content = encoding.GetPreamble().Concat(encoding.GetBytes(csv.ToString())).ToArray();
        return File(content, "text/csv; charset=utf-8", $"payments-{DateTime.UtcNow:yyyyMMdd}.csv");
    }

    private static GetPaymentsQuery BuildQuery(
        int page,
        int pageSize,
        string? search,
        string? sortBy,
        string? sortDir,
        Guid? residentId,
        PaymentStatus? status,
        PaymentMethod? method,
        DateTime? from,
        DateTime? to,
        decimal? minAmount,
        decimal? maxAmount) =>
        new(
            new PaginationQuery(page, pageSize, search, sortBy, sortDir),
            residentId,
            status,
            method,
            from,
            to,
            minAmount,
            maxAmount);

    private static string Csv(string value) =>
        $"\"{value.Replace("\"", "\"\"", StringComparison.Ordinal)}\"";
}
