using FluentValidation;

namespace BehaAtel.Application.Features.Admins.DeleteAdmin;

public sealed class DeleteAdminValidator : AbstractValidator<DeleteAdminCommand>
{
    public DeleteAdminValidator()
    {
        RuleFor(command => command.AdminId).NotEmpty();
    }
}
