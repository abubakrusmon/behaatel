using BehaAtel.Application.Common.Validation;
using FluentValidation;

namespace BehaAtel.Application.Features.Admins.CreateAdmin;

public sealed class CreateAdminValidator : AbstractValidator<CreateAdminCommand>
{
    public CreateAdminValidator()
    {
        RuleFor(command => command.Email).NotEmpty().EmailAddress();
        RuleFor(command => command.FullName).NotEmpty().MaximumLength(200);
        RuleFor(command => command.Phone).MaximumLength(50);
        RuleFor(command => command.Password).RequireStrongPassword();
        RuleFor(command => command.Permissions).NotNull();
        RuleForEach(command => command.Permissions).IsInEnum();
    }
}
