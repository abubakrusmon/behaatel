using BehaAtel.Domain.Enums;
using FluentValidation;

namespace BehaAtel.Application.Features.Payments.CreatePayment;

public sealed class CreatePaymentValidator : AbstractValidator<CreatePaymentCommand>
{
    public CreatePaymentValidator()
    {
        RuleFor(command => command.ResidentId).NotEmpty();
        RuleFor(command => command.Amount).GreaterThan(0);
        RuleFor(command => command.Method).IsInEnum();
        RuleFor(command => command.PaidAt)
            .Must(paidAt => !paidAt.HasValue || paidAt.Value.Kind == DateTimeKind.Utc)
            .WithMessage("Дата платежа должна быть в UTC.");
        RuleFor(command => command.Comment).MaximumLength(1000);
    }
}
