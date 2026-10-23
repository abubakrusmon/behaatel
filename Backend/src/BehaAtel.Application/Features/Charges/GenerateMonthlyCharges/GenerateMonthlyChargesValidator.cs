using FluentValidation;

namespace BehaAtel.Application.Features.Charges.GenerateMonthlyCharges;

public sealed class GenerateMonthlyChargesValidator : AbstractValidator<GenerateMonthlyChargesCommand>
{
    public GenerateMonthlyChargesValidator()
    {
        RuleFor(command => command.Period).Must(period => period != default);
    }
}
