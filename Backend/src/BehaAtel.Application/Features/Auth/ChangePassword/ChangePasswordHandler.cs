using BehaAtel.Application.Common.Exceptions;
using BehaAtel.Application.Common.Interfaces;
using BehaAtel.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BehaAtel.Application.Features.Auth.ChangePassword;

public sealed class ChangePasswordHandler(
    IApplicationDbContext dbContext,
    ICurrentUser currentUser,
    IPasswordHasher passwordHasher,
    IAuditService auditService)
    : IRequestHandler<ChangePasswordCommand>
{
    public async Task Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } userId)
        {
            throw new UnauthorizedException("Пользователь не авторизован.");
        }

        var user = await dbContext.Users
            .Include(candidate => candidate.RefreshTokens)
            .SingleOrDefaultAsync(candidate => candidate.Id == userId, cancellationToken);

        if (user is null || !user.IsActive || user.IsDeleted)
        {
            throw new UnauthorizedException("Пользователь не авторизован.");
        }

        if (!passwordHasher.Verify(user.PasswordHash, request.CurrentPassword))
        {
            throw new UnauthorizedException("Текущий пароль неверен.");
        }

        user.SetPassword(passwordHasher.Hash(request.NewPassword), mustChange: false);
        foreach (var token in user.RefreshTokens.Where(token => token.IsActive))
        {
            token.Revoke();
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        await auditService.LogAsync("ChangePassword", nameof(User), user.Id.ToString(), null, null, cancellationToken);
    }
}
