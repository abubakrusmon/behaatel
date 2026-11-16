using BehaAtel.Domain.Entities;
using BehaAtel.Domain.Enums;

namespace BehaAtel.Application.Features.Auth.Common;

public sealed record UserInfoDto(
    Guid Id,
    string Email,
    string FullName,
    Role Role,
    Permission[] Permissions,
    bool MustChangePassword,
    string? AvatarUrl = null)
{
    public static UserInfoDto FromUser(User user) =>
        new(user.Id, user.Email, user.FullName, user.Role,
            user.Role == Role.SuperAdmin
                ? Enum.GetValues<Permission>()
                : user.Permissions.Select(permission => permission.Permission).ToArray(),
            user.MustChangePassword,
            user.AvatarData);
}
