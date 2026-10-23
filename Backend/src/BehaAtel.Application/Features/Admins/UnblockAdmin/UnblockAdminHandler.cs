using BehaAtel.Application.Common.Exceptions;
using BehaAtel.Application.Common.Interfaces;
using BehaAtel.Application.Features.Admins.Common;
using BehaAtel.Domain.Entities;
using BehaAtel.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BehaAtel.Application.Features.Admins.UnblockAdmin;

public sealed class UnblockAdminHandler(
    IApplicationDbContext dbContext,
    ICurrentUser currentUser,
    IAuditService auditService)
    : IRequestHandler<UnblockAdminCommand>
{
    public async Task Handle(UnblockAdminCommand request, CancellationToken cancellationToken)
    {
        AdminAuthorization.RequireSuperAdmin(currentUser);
        AdminAuthorization.RequireNotSelf(currentUser, request.AdminId);
        var admin = await dbContext.Users
            .SingleOrDefaultAsync(user => user.Id == request.AdminId && !user.IsDeleted, cancellationToken)
            ?? throw new NotFoundException(nameof(User), request.AdminId);

        if (admin.Role != Role.Admin)
        {
            throw new ForbiddenException("Нельзя разблокировать SuperAdmin.");
        }

        var wasActive = admin.IsActive;
        admin.Unblock();
        await dbContext.SaveChangesAsync(cancellationToken);
        await auditService.LogAsync("UnblockAdmin", nameof(User), admin.Id.ToString(),
            new { IsActive = wasActive }, new { admin.IsActive }, cancellationToken);
    }
}
