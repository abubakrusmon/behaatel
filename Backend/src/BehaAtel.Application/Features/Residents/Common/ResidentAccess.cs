using BehaAtel.Application.Common.Interfaces;
using BehaAtel.Domain.Entities;
using BehaAtel.Domain.Enums;

namespace BehaAtel.Application.Features.Residents.Common;

internal static class ResidentAccess
{
    public static IQueryable<Resident> ScopeToCurrentUser(IQueryable<Resident> query, ICurrentUser currentUser)
    {
        if (currentUser.Role != Role.User)
        {
            return query;
        }

        return currentUser.UserId is { } userId
            ? query.Where(resident => resident.UserId == userId)
            : query.Where(_ => false);
    }
}
