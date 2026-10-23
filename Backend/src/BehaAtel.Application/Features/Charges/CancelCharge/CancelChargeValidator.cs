using FluentValidation;

namespace BehaAtel.Application.Features.Charges.CancelCharge;

public sealed class CancelChargeValidator : AbstractValidator<CancelChargeCommand>
{
    public CancelChargeValidator()
    {
        RuleFor(command => command.ChargeId).NotEmpty();
        RuleFor(command => command.Reason).NotEmpty().MaximumLength(1000);
    }
}
