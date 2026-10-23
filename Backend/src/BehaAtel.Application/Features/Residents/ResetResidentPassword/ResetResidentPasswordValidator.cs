using BehaAtel.Application.Common.Validation;
using FluentValidation;

namespace BehaAtel.Application.Features.Residents.ResetResidentPassword;

public sealed class ResetResidentPasswordValidator : AbstractValidator<ResetResidentPasswordCommand>
{
    public ResetResidentPasswordValidator()
    {
        RuleFor(command => command.ResidentId).NotEmpty();
        RuleFor(command => command.NewPassword).RequireStrongPassword();
    }
}
