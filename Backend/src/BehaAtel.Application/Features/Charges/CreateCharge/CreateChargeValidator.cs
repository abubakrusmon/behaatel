using BehaAtel.Domain.Enums;
using FluentValidation;

namespace BehaAtel.Application.Features.Charges.CreateCharge;

public sealed class CreateChargeValidator : AbstractValidator<CreateChargeCommand>
{
    public CreateChargeValidator()
    {
        RuleFor(command => command.ResidentId).NotEmpty();
        RuleFor(command => command.Type).IsInEnum();
        RuleFor(command => command.Amount).GreaterThan(0);
        RuleFor(command => command.Description).MaximumLength(1000);
    }
}
