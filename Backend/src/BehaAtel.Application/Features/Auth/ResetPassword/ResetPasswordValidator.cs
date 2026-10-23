using BehaAtel.Application.Common.Validation;
using FluentValidation;

namespace BehaAtel.Application.Features.Auth.ResetPassword;

public sealed class ResetPasswordValidator : AbstractValidator<ResetPasswordCommand>
{
    public ResetPasswordValidator()
    {
        RuleFor(request => request.Email).NotEmpty().EmailAddress().MaximumLength(320);
        RuleFor(request => request.Code).Matches("^\\d{6}$");
        RuleFor(request => request.NewPassword).RequireStrongPassword();
    }
}
