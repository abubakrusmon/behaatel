using FluentValidation;

namespace BehaAtel.Application.Features.Auth.Refresh;

public sealed class RefreshTokenValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenValidator()
    {
        RuleFor(command => command.RefreshToken).NotEmpty();
    }
}
