using System.Globalization;
using System.Security.Cryptography;
using BehaAtel.Application.Common.Exceptions;
using BehaAtel.Application.Common.Interfaces;
using BehaAtel.Application.Features.Payments.Common;
using BehaAtel.Domain.Entities;
using BehaAtel.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BehaAtel.Application.Features.Payments.CreatePayment;

public sealed class CreatePaymentHandler(
    IApplicationDbContext dbContext,
    IDateTimeProvider dateTimeProvider,
    IAuditService auditService)
    : IRequestHandler<CreatePaymentCommand, PaymentDto>
{
    public async Task<PaymentDto> Handle(CreatePaymentCommand request, CancellationToken cancellationToken)
    {
        var resident = await dbContext.Residents
            .AsNoTracking()
            .Where(candidate => candidate.Id == request.ResidentId && !candidate.IsDeleted)
            .Join(
                dbContext.Users.AsNoTracking().Where(user => !user.IsDeleted),
                candidate => candidate.UserId,
                user => user.Id,
                (candidate, user) => new { candidate.Id, user.FullName })
            .SingleOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException(nameof(Resident), request.ResidentId);

        var paidAt = request.PaidAt ?? dateTimeProvider.UtcNow;
        var receiptNumber = await GenerateReceiptNumberAsync(paidAt, cancellationToken);
        var payment = Payment.Create(
            request.ResidentId, request.Amount, request.Method, paidAt, receiptNumber, request.Comment);
        var charges = await dbContext.Charges
            .Include(charge => charge.Allocations)
            .Where(charge => charge.ResidentId == request.ResidentId &&
                             charge.Status != ChargeStatus.Cancelled &&
                             charge.Amount > (dbContext.PaymentAllocations
                                 .Where(allocation => allocation.ChargeId == charge.Id)
                                 .Sum(allocation => (decimal?)allocation.Amount) ?? 0m))
            .OrderBy(charge => charge.DueDate)
            .ThenBy(charge => charge.Period)
            .ToListAsync(cancellationToken);

        var remainingPayment = request.Amount;
        foreach (var charge in charges)
        {
            if (remainingPayment <= 0)
            {
                break;
            }

            var allocationAmount = Math.Min(remainingPayment, charge.RemainingAmount);
            if (allocationAmount <= 0)
            {
                continue;
            }

            var allocation = PaymentAllocation.Create(payment.Id, charge.Id, allocationAmount);
            payment.Allocate(allocation);
            charge.Allocate(allocation);
            dbContext.PaymentAllocations.Add(allocation);
            remainingPayment -= allocationAmount;
        }

        dbContext.Payments.Add(payment);
        await dbContext.SaveChangesAsync(cancellationToken);
        await auditService.LogAsync("CreatePayment", nameof(Payment), payment.Id.ToString(), null,
            new
            {
                payment.ResidentId,
                payment.Amount,
                payment.Method,
                payment.PaidAt,
                payment.ReceiptNumber,
                payment.AllocatedAmount,
                payment.UnallocatedAmount
            }, cancellationToken);

        return new PaymentDto(
            payment.Id,
            payment.ResidentId,
            resident.FullName,
            payment.Amount,
            payment.Method,
            payment.Status,
            payment.PaidAt,
            payment.ReceiptNumber,
            payment.Comment,
            payment.AllocatedAmount,
            payment.UnallocatedAmount);
    }

    private async Task<string> GenerateReceiptNumberAsync(DateTime paidAt, CancellationToken cancellationToken)
    {
        for (var attempt = 0; attempt < 100; attempt++)
        {
            var suffix = RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6", CultureInfo.InvariantCulture);
            var receiptNumber = $"BA-{paidAt:yyyyMMdd}-{suffix}";
            if (!await dbContext.Payments.AnyAsync(
                    payment => payment.ReceiptNumber == receiptNumber, cancellationToken))
            {
                return receiptNumber;
            }
        }

        throw new ConflictException("Не удалось сгенерировать уникальный номер квитанции.");
    }
}
