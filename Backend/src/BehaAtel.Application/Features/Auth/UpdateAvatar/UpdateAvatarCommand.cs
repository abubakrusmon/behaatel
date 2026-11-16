using BehaAtel.Application.Features.Auth.Common;
using MediatR;

namespace BehaAtel.Application.Features.Auth.UpdateAvatar;

/// <param name="AvatarUrl">data URL изображения; null или пустая строка — удалить аватар.</param>
public sealed record UpdateAvatarCommand(string? AvatarUrl) : IRequest<UserInfoDto>;
