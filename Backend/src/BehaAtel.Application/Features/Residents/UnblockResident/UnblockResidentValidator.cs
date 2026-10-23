using FluentValidation;

namespace BehaAtel.Application.Features.Residents.UnblockResident;

public sealed class UnblockResidentValidator : AbstractValidator<UnblockResidentCommand>
{
    public UnblockResidentValidator()
    {
        RuleFor(command => command.ResidentId).NotEmpty();
    }
}
