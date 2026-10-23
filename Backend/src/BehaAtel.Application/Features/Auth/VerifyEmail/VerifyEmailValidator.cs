using FluentValidation;

namespace BehaAtel.Application.Features.Auth.VerifyEmail;

public sealed class VerifyEmailValidator : AbstractValidator<VerifyEmailCommand>
{
    public VerifyEmailValidator()
    {
        RuleFor(request => request.Email).NotEmpty().EmailAddress().MaximumLength(320);
        RuleFor(request => request.Code).Matches("^\\d{6}$");
    }
}
