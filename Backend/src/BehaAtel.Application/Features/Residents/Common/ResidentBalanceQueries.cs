using BehaAtel.Domain.Entities;
using BehaAtel.Domain.Enums;

namespace BehaAtel.Application.Features.Residents.Common;

public static class ResidentBalanceQueries
{
    public static decimal Calculate(decimal paid, decimal charged) => paid - charged;

    public static IQueryable<ResidentBalance> WithBalance(
        this IQueryable<Resident> residents,
        IQueryable<Payment> payments,
        IQueryable<Charge> charges) =>
        residents.Select(resident => new ResidentBalance(
            resident,
            (payments
                .Where(payment => payment.ResidentId == resident.Id && payment.Status == PaymentStatus.Completed)
                .Sum(payment => (decimal?)payment.Amount) ?? 0m)
            - (charges
                .Where(charge => charge.ResidentId == resident.Id && charge.Status != ChargeStatus.Cancelled)
                .Sum(charge => (decimal?)charge.Amount) ?? 0m)));

    public static IQueryable<ResidentBalance> WhereHasDebt(
        this IQueryable<Resident> residents,
        IQueryable<Payment> payments,
        IQueryable<Charge> charges) =>
        residents.WithDebt(payments, charges).WithBalance(payments, charges);

    public static IQueryable<ResidentBalance> WhereHasNoDebt(
        this IQueryable<Resident> residents,
        IQueryable<Payment> payments,
        IQueryable<Charge> charges) =>
        residents.WithNoDebt(payments, charges).WithBalance(payments, charges);

    public static IQueryable<Resident> WithDebt(
        this IQueryable<Resident> residents,
        IQueryable<Payment> payments,
        IQueryable<Charge> charges) =>
        residents.Where(resident =>
            (payments
                .Where(payment => payment.ResidentId == resident.Id && payment.Status == PaymentStatus.Completed)
                .Sum(payment => (decimal?)payment.Amount) ?? 0m)
            - (charges
                .Where(charge => charge.ResidentId == resident.Id && charge.Status != ChargeStatus.Cancelled)
                .Sum(charge => (decimal?)charge.Amount) ?? 0m) < 0);

    public static IQueryable<Resident> WithNoDebt(
        this IQueryable<Resident> residents,
        IQueryable<Payment> payments,
        IQueryable<Charge> charges) =>
        residents.Where(resident =>
            (payments
                .Where(payment => payment.ResidentId == resident.Id && payment.Status == PaymentStatus.Completed)
                .Sum(payment => (decimal?)payment.Amount) ?? 0m)
            - (charges
                .Where(charge => charge.ResidentId == resident.Id && charge.Status != ChargeStatus.Cancelled)
                .Sum(charge => (decimal?)charge.Amount) ?? 0m) >= 0);
}
