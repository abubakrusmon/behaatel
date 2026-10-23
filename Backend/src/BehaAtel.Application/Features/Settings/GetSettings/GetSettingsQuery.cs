using BehaAtel.Application.Features.Settings.Common;
using MediatR;

namespace BehaAtel.Application.Features.Settings.GetSettings;

public sealed record GetSettingsQuery : IRequest<IReadOnlyCollection<SettingDto>>;
