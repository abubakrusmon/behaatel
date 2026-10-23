using FluentValidation;

namespace BehaAtel.Application.Features.Payments.CancelPayment;

public sealed class CancelPaymentValidator : AbstractValidator<CancelPaymentCommand>
{
    public CancelPaymentValidator()
    {
        RuleFor(command => command.PaymentId).NotEmpty();
        RuleFor(command => command.Reason).NotEmpty().MaximumLength(1000);
    }
}
