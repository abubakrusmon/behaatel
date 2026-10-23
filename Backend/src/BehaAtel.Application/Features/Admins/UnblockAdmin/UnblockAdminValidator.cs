using FluentValidation;

namespace BehaAtel.Application.Features.Admins.UnblockAdmin;

public sealed class UnblockAdminValidator : AbstractValidator<UnblockAdminCommand>
{
    public UnblockAdminValidator()
    {
        RuleFor(command => command.AdminId).NotEmpty();
    }
}
