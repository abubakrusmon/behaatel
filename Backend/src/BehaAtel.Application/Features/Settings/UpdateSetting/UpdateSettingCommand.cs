using BehaAtel.Application.Features.Settings.Common;
using MediatR;

namespace BehaAtel.Application.Features.Settings.UpdateSetting;

public sealed record UpdateSettingCommand(string Key, string Value) : IRequest<SettingDto>;
