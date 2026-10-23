using FluentValidation;
using BehaAtel.Application.Common.Validation;

namespace BehaAtel.Application.Features.Auth.ChangePassword;

public sealed class ChangePasswordValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordValidator()
    {
        RuleFor(command => command.CurrentPassword).NotEmpty();
        RuleFor(command => command.NewPassword).RequireStrongPassword();
    }
}
