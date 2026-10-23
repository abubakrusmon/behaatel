using BehaAtel.Application.Common.Interfaces;
using BehaAtel.Application.Common.Validation;
using FluentValidation;

namespace BehaAtel.Application.Features.Auth.Register;

public sealed class RegisterValidator : AbstractValidator<RegisterCommand>
{
    public RegisterValidator(IDateTimeProvider clock)
    {
        RuleFor(command => command.Email).NotEmpty().EmailAddress().MaximumLength(320);
        RuleFor(command => command.FullName).NotEmpty().MaximumLength(200);
        RuleFor(command => command.Phone).MaximumLength(50);
        RuleFor(command => command.Password).RequireStrongPassword();
        RuleFor(command => command.PassportNumber).NotEmpty().MaximumLength(100);
        RuleFor(command => command.Organization).MaximumLength(300);
        RuleFor(command => command.BirthDate)
            .Must(value => value <= DateOnly.FromDateTime(clock.UtcNow).AddYears(-14))
            .WithMessage("Вам должно быть не меньше 14 лет.");
    }
}
