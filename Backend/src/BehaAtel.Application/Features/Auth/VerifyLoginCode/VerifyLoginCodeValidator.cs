using FluentValidation;

namespace BehaAtel.Application.Features.Auth.VerifyLoginCode;

public sealed class VerifyLoginCodeValidator : AbstractValidator<VerifyLoginCodeCommand>
{
    public VerifyLoginCodeValidator()
    {
        RuleFor(command => command.ChallengeId).NotEmpty();
        RuleFor(command => command.Code).Matches("^\\d{6}$");
    }
}
