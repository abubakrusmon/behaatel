using BehaAtel.Application.Common.Exceptions;
using BehaAtel.Application.Common.Interfaces;
using BehaAtel.Application.Features.Auth.Common;
using BehaAtel.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BehaAtel.Application.Features.Auth.Refresh;

public sealed class RefreshTokenHandler(
    IApplicationDbContext dbContext,
    IJwtTokenService jwtTokenService,
    ICurrentUser currentUser,
    IAuditService auditService,
    ITelegramAccountLinkService telegramLinkService)
    : IRequestHandler<RefreshTokenCommand, AuthResponse>
{
    public async Task<AuthResponse> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var token = await dbContext.RefreshTokens
            .SingleOrDefaultAsync(candidate => candidate.Token == request.RefreshToken, cancellationToken);

        if (token is null)
        {
            throw new UnauthorizedException("Токен недействителен.");
        }

        if (token.IsRevoked)
        {
            var activeTokens = await dbContext.RefreshTokens
                .Where(candidate => candidate.UserId == token.UserId && candidate.RevokedAt == null)
                .ToListAsync(cancellationToken);
            foreach (var activeToken in activeTokens)
            {
                if (activeToken.IsActive)
                {
                    activeToken.Revoke();
                }
            }

            await dbContext.SaveChangesAsync(cancellationToken);
            throw new UnauthorizedException("Токен недействителен.");
        }

        if (!token.IsActive)
        {
            throw new UnauthorizedException("Токен недействителен.");
        }

        var user = await dbContext.Users
            .Include(candidate => candidate.Permissions)
            .SingleOrDefaultAsync(candidate => candidate.Id == token.UserId, cancellationToken);

        if (user is null || !user.IsActive || user.IsDeleted)
        {
            throw new UnauthorizedException("Токен недействителен.");
        }
        if (user.RequiresTelegramLink && telegramLinkService.IsConfigured)
        {
            throw new UnauthorizedException("Сначала завершите привязку Telegram.");
        }

        var replacement = jwtTokenService.GenerateRefreshToken(user.Id, currentUser.IpAddress);
        token.Revoke(replacement.Token);
        dbContext.RefreshTokens.Add(replacement);
        await dbContext.SaveChangesAsync(cancellationToken);
        await auditService.LogAsync("RefreshToken", nameof(User), user.Id.ToString(), null, null, cancellationToken);

        return new AuthResponse(
            jwtTokenService.GenerateAccessToken(user),
            replacement.Token,
            replacement.ExpiresAt,
            UserInfoDto.FromUser(user));
    }
}
