using BehaAtel.Application.Common.Exceptions;
using BehaAtel.Application.Common.Interfaces;
using BehaAtel.Domain.Enums;

namespace BehaAtel.Application.Features.Admins.Common;

internal static class AdminAuthorization
{
    public static void RequireSuperAdmin(ICurrentUser currentUser)
    {
        if (currentUser.Role != Role.SuperAdmin)
        {
            throw new ForbiddenException();
        }
    }

    public static void RequireNotSelf(ICurrentUser currentUser, Guid adminId)
    {
        if (currentUser.UserId == adminId)
        {
            throw new ForbiddenException("Нельзя изменять собственную учётную запись через управление администраторами.");
        }
    }
}
