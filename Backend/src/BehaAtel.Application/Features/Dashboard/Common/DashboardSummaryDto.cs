namespace BehaAtel.Application.Features.Dashboard.Common;

public sealed record DashboardSummaryDto(
    int ActiveResidents,
    int TotalRooms,
    int TotalBeds,
    int OccupiedBeds,
    decimal OccupancyPercent,
    decimal IncomeThisMonth,
    decimal IncomeLastMonth,
    decimal ChargedThisMonth,
    decimal TotalDebt,
    int DebtorsCount);
