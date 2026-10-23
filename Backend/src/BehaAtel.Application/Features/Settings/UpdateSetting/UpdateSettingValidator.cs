using BehaAtel.Application.Features.Settings.Common;
using FluentValidation;

namespace BehaAtel.Application.Features.Settings.UpdateSetting;

public sealed class UpdateSettingValidator : AbstractValidator<UpdateSettingCommand>
{
    public UpdateSettingValidator()
    {
        RuleFor(command => command.Key)
            .NotEmpty()
            .Must(key => SettingKeys.IsKnown(key?.Trim()))
            .WithMessage("Неизвестный ключ настройки.");
        RuleFor(command => command.Value)
            .NotEmpty()
            .Must((command, value) => SettingKeys.IsValidValue(command.Key?.Trim(), value))
            .WithMessage("Значение не соответствует формату выбранной настройки.");
    }
}
