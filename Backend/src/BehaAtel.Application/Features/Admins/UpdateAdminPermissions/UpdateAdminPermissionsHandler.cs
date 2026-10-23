using BehaAtel.Application.Common.Exceptions;
using BehaAtel.Application.Common.Interfaces;
using BehaAtel.Application.Features.Admins.Common;
using BehaAtel.Domain.Entities;
using BehaAtel.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BehaAtel.Application.Features.Admins.UpdateAdminPermissions;

public sealed class UpdateAdminPermissionsHandler(
    IApplicationDbContext dbContext,
    ICurrentUser currentUser,
    IAuditService auditService)
    : IRequestHandler<UpdateAdminPermissionsCommand>
{
    public async Task Handle(UpdateAdminPermissionsCommand request, CancellationToken cancellationToken)
    {
        AdminAuthorization.RequireSuperAdmin(currentUser);
        var admin = await dbContext.Users
            .Include(user => user.Permissions)
            .SingleOrDefaultAsync(user => user.Id == request.AdminId && !user.IsDeleted, cancellationToken)
            ?? throw new NotFoundException(nameof(User), request.AdminId);

        if (admin.Role != Role.Admin)
        {
            throw new ForbiddenException("Нельзя изменять права SuperAdmin.");
        }

        var oldPermissions = admin.Permissions.Select(permission => permission.Permission).ToArray();
        admin.SetPermissions(request.Permissions);
        await dbContext.SaveChangesAsync(cancellationToken);
        await auditService.LogAsync("UpdateAdminPermissions", nameof(User), admin.Id.ToString(),
            new { Permissions = oldPermissions }, new { Permissions = request.Permissions }, cancellationToken);
    }
}
