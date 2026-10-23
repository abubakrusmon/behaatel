using BehaAtel.Application.Common.Exceptions;
using BehaAtel.Application.Common.Interfaces;
using BehaAtel.Application.Features.Admins.Common;
using BehaAtel.Domain.Entities;
using BehaAtel.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BehaAtel.Application.Features.Admins.DeleteAdmin;

public sealed class DeleteAdminHandler(
    IApplicationDbContext dbContext,
    ICurrentUser currentUser,
    IAuditService auditService)
    : IRequestHandler<DeleteAdminCommand>
{
    public async Task Handle(DeleteAdminCommand request, CancellationToken cancellationToken)
    {
        AdminAuthorization.RequireSuperAdmin(currentUser);
        AdminAuthorization.RequireNotSelf(currentUser, request.AdminId);
        var admin = await dbContext.Users
            .Include(user => user.RefreshTokens)
            .SingleOrDefaultAsync(user => user.Id == request.AdminId && !user.IsDeleted, cancellationToken)
            ?? throw new NotFoundException(nameof(User), request.AdminId);

        if (admin.Role != Role.Admin)
        {
            throw new ForbiddenException("Нельзя удалить SuperAdmin.");
        }

        admin.Delete();
        foreach (var token in admin.RefreshTokens.Where(token => token.IsActive))
        {
            token.Revoke();
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        await auditService.LogAsync("DeleteAdmin", nameof(User), admin.Id.ToString(),
            new { IsDeleted = false }, new { admin.IsDeleted }, cancellationToken);
    }
}
