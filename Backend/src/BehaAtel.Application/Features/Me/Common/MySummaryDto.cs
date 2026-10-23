namespace BehaAtel.Application.Features.Me.Common;

public sealed record MySummaryDto(
    decimal DueThisMonth,
    decimal Balance,
    decimal TotalDebt,
    DateOnly? NextDueDate,
    MyLastPaymentDto? LastPayment);
