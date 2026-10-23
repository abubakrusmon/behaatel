using BehaAtel.Domain.Entities;
using BehaAtel.Domain.Enums;

namespace BehaAtel.Application.Features.Admins.Common;

public sealed record AdminDto(
    Guid Id,
    string Email,
    string FullName,
    string? Phone,
    bool IsActive,
    Permission[] Permissions,
    DateTime? LastLoginAt,
    DateTime CreatedAt)
{
    public static AdminDto FromUser(User user) =>
        new(user.Id, user.Email, user.FullName, user.Phone, user.IsActive,
            user.Permissions.Select(permission => permission.Permission).ToArray(),
            user.LastLoginAt, user.CreatedAt);
}
