using FluentValidation;

namespace BehaAtel.Application.Features.Admins.UpdateAdminPermissions;

public sealed class UpdateAdminPermissionsValidator : AbstractValidator<UpdateAdminPermissionsCommand>
{
    public UpdateAdminPermissionsValidator()
    {
        RuleFor(command => command.AdminId).NotEmpty();
        RuleFor(command => command.Permissions).NotNull();
        RuleForEach(command => command.Permissions).IsInEnum();
    }
}
