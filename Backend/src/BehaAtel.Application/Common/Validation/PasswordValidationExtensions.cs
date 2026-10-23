using FluentValidation;

namespace BehaAtel.Application.Common.Validation;

public static class PasswordValidationExtensions
{
    public static IRuleBuilderOptions<T, string> RequireStrongPassword<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .MinimumLength(8)
            .Matches("[A-Z]").WithMessage("Пароль должен содержать заглавную букву.")
            .Matches("[a-z]").WithMessage("Пароль должен содержать строчную букву.")
            .Matches("[0-9]").WithMessage("Пароль должен содержать цифру.")
            .Matches("[^A-Za-z0-9]").WithMessage("Пароль должен содержать специальный символ.");
    }
}
