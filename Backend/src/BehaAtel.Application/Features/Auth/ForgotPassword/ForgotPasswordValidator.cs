using FluentValidation;

namespace BehaAtel.Application.Features.Auth.ForgotPassword;

public sealed class ForgotPasswordValidator : AbstractValidator<ForgotPasswordCommand>
{
    public ForgotPasswordValidator() =>
        RuleFor(command => command.Email).NotEmpty().EmailAddress().MaximumLength(320);
}
