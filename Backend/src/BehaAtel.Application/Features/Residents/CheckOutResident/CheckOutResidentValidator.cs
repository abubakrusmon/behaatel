using FluentValidation;

namespace BehaAtel.Application.Features.Residents.CheckOutResident;

public sealed class CheckOutResidentValidator : AbstractValidator<CheckOutResidentCommand>
{
    public CheckOutResidentValidator()
    {
        RuleFor(command => command.ResidentId).NotEmpty();
    }
}
