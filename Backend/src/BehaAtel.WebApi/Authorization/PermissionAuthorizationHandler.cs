using System.Security.Claims;
using BehaAtel.Application.Common.Security;
using BehaAtel.Domain.Enums;
using Microsoft.AspNetCore.Authorization;

namespace BehaAtel.WebApi.Authorization;

public sealed class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        if (context.User.IsInRole(nameof(Role.SuperAdmin)) ||
            context.User.HasClaim("permission", PermissionNames.ToClaimValue(requirement.Permission)))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
