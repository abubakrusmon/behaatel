using BehaAtel.Application.Common.Interfaces;
using BehaAtel.Application.Features.Settings.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BehaAtel.Application.Features.Settings.GetSettings;

public sealed class GetSettingsHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetSettingsQuery, IReadOnlyCollection<SettingDto>>
{
    public async Task<IReadOnlyCollection<SettingDto>> Handle(
        GetSettingsQuery request,
        CancellationToken cancellationToken)
    {
        var settings = await dbContext.Settings.AsNoTracking()
            .Where(setting => SettingKeys.All.Contains(setting.Key))
            .Select(setting => new SettingDto(setting.Key, setting.Value, setting.UpdatedAt))
            .ToListAsync(cancellationToken);
        var settingsByKey = settings.ToDictionary(setting => setting.Key);
        var defaultUpdatedAt = DateTime.SpecifyKind(DateTime.MinValue, DateTimeKind.Utc);

        return SettingKeys.All
            .Select(key => settingsByKey.TryGetValue(key, out var setting)
                ? setting
                : new SettingDto(key, SettingKeys.DefaultValue(key), defaultUpdatedAt))
            .ToArray();
    }
}
