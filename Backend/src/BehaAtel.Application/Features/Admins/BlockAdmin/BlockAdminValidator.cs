using FluentValidation;

namespace BehaAtel.Application.Features.Admins.BlockAdmin;

public sealed class BlockAdminValidator : AbstractValidator<BlockAdminCommand>
{
    public BlockAdminValidator()
    {
        RuleFor(command => command.AdminId).NotEmpty();
    }
}
