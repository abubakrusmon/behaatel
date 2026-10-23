using BehaAtel.Application.Common.Exceptions;
using BehaAtel.Application.Common.Interfaces;
using BehaAtel.Application.Features.Settings.Common;
using BehaAtel.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BehaAtel.Application.Features.Settings.UpdateSetting;

public sealed class UpdateSettingHandler(
    IApplicationDbContext dbContext,
    IDateTimeProvider dateTimeProvider,
    IAuditService auditService)
    : IRequestHandler<UpdateSettingCommand, SettingDto>
{
    public async Task<SettingDto> Handle(UpdateSettingCommand request, CancellationToken cancellationToken)
    {
        var key = request.Key.Trim();
        if (!SettingKeys.IsKnown(key))
        {
            throw new ValidationException(new Dictionary<string, string[]>
            {
                [nameof(request.Key)] = ["Неизвестный ключ настройки."]
            });
        }

        if (!SettingKeys.IsValidValue(key, request.Value))
        {
            throw new ValidationException(new Dictionary<string, string[]>
            {
                [nameof(request.Value)] = ["Значение не соответствует формату выбранной настройки."]
            });
        }

        var setting = await dbContext.Settings
            .SingleOrDefaultAsync(candidate => candidate.Key == key, cancellationToken);
        var oldValue = setting?.Value;
        var now = dateTimeProvider.UtcNow;

        if (setting is null)
        {
            setting = Setting.Create(key, request.Value, now);
            dbContext.Settings.Add(setting);
        }
        else
        {
            setting.UpdateValue(request.Value, now);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        await auditService.LogAsync("UpdateSetting", nameof(Setting), key,
            new { Value = oldValue }, new { setting.Value }, cancellationToken);

        return new SettingDto(setting.Key, setting.Value, setting.UpdatedAt);
    }
}
