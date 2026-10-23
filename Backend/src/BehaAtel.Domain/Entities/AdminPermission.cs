using BehaAtel.Domain.Enums;
using BehaAtel.Domain.Exceptions;

namespace BehaAtel.Domain.Entities;

public class AdminPermission
{
    private AdminPermission()
    {
    }

    private AdminPermission(Guid userId, Permission permission)
    {
        if (userId == Guid.Empty)
        {
            throw new DomainException("UserId не может быть пустым.");
        }

        if (!Enum.IsDefined(permission))
        {
            throw new DomainException("Указано неизвестное право.");
        }

        UserId = userId;
        Permission = permission;
    }

    public Guid UserId { get; private set; }
    public Permission Permission { get; private set; }

    public static AdminPermission Create(Guid userId, Permission permission) =>
        new(userId, permission);
}
