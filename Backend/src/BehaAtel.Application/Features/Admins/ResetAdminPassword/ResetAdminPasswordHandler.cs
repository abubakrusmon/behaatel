using BehaAtel.Application.Common.Exceptions;
using BehaAtel.Application.Common.Interfaces;
using BehaAtel.Application.Features.Admins.Common;
using BehaAtel.Domain.Entities;
using BehaAtel.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BehaAtel.Application.Features.Admins.ResetAdminPassword;

public sealed class ResetAdminPasswordHandler(
    IApplicationDbContext dbContext,
    ICurrentUser currentUser,
    IPasswordHasher passwordHasher,
    IAuditService auditService)
    : IRequestHandler<ResetAdminPasswordCommand>
{
    public async Task Handle(ResetAdminPasswordCommand request, CancellationToken cancellationToken)
    {
        AdminAuthorization.RequireSuperAdmin(currentUser);
        var admin = await dbContext.Users
            .Include(user => user.RefreshTokens)
            .SingleOrDefaultAsync(user => user.Id == request.AdminId && !user.IsDeleted, cancellationToken)
            ?? throw new NotFoundException(nameof(User), request.AdminId);

        if (admin.Role != Role.Admin)
        {
            throw new ForbiddenException("Нельзя сбросить пароль SuperAdmin.");
        }

        admin.SetPassword(passwordHasher.Hash(request.NewPassword), mustChange: true);
        foreach (var token in admin.RefreshTokens.Where(token => token.IsActive))
        {
            token.Revoke();
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        await auditService.LogAsync("ResetAdminPassword", nameof(User), admin.Id.ToString(), null,
            new { MustChangePassword = true }, cancellationToken);
    }
}
