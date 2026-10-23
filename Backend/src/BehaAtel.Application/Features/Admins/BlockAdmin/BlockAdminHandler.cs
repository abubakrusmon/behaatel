using BehaAtel.Application.Common.Exceptions;
using BehaAtel.Application.Common.Interfaces;
using BehaAtel.Application.Features.Admins.Common;
using BehaAtel.Domain.Entities;
using BehaAtel.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BehaAtel.Application.Features.Admins.BlockAdmin;

public sealed class BlockAdminHandler(
    IApplicationDbContext dbContext,
    ICurrentUser currentUser,
    IAuditService auditService)
    : IRequestHandler<BlockAdminCommand>
{
    public async Task Handle(BlockAdminCommand request, CancellationToken cancellationToken)
    {
        AdminAuthorization.RequireSuperAdmin(currentUser);
        AdminAuthorization.RequireNotSelf(currentUser, request.AdminId);
        var admin = await dbContext.Users
            .Include(user => user.RefreshTokens)
            .SingleOrDefaultAsync(user => user.Id == request.AdminId && !user.IsDeleted, cancellationToken)
            ?? throw new NotFoundException(nameof(User), request.AdminId);

        if (admin.Role != Role.Admin)
        {
            throw new ForbiddenException("Нельзя блокировать SuperAdmin.");
        }

        var wasActive = admin.IsActive;
        admin.Block();
        foreach (var token in admin.RefreshTokens.Where(token => token.IsActive))
        {
            token.Revoke();
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        await auditService.LogAsync("BlockAdmin", nameof(User), admin.Id.ToString(),
            new { IsActive = wasActive }, new { admin.IsActive }, cancellationToken);
    }
}
