using BehaAtel.Application.Common.Validation;
using FluentValidation;

namespace BehaAtel.Application.Features.Admins.ResetAdminPassword;

public sealed class ResetAdminPasswordValidator : AbstractValidator<ResetAdminPasswordCommand>
{
    public ResetAdminPasswordValidator()
    {
        RuleFor(command => command.AdminId).NotEmpty();
        RuleFor(command => command.NewPassword).RequireStrongPassword();
    }
}
