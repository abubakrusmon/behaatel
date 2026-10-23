using FluentValidation;

namespace BehaAtel.Application.Features.Auth.ResendVerification;

public sealed class ResendVerificationValidator : AbstractValidator<ResendVerificationCommand>
{
    public ResendVerificationValidator() =>
        RuleFor(command => command.Email).NotEmpty().EmailAddress().MaximumLength(320);
}
