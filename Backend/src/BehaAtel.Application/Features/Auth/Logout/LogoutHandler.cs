using BehaAtel.Application.Common.Exceptions;
using BehaAtel.Application.Common.Interfaces;
using BehaAtel.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BehaAtel.Application.Features.Auth.Logout;

public sealed class LogoutHandler(IApplicationDbContext dbContext, IAuditService auditService)
    : IRequestHandler<LogoutCommand>
{
    public async Task Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        var token = await dbContext.RefreshTokens
            .SingleOrDefaultAsync(candidate => candidate.Token == request.RefreshToken, cancellationToken);

        if (token is null)
        {
            throw new UnauthorizedException("Токен недействителен.");
        }

        if (!token.IsRevoked)
        {
            token.Revoke();
            await dbContext.SaveChangesAsync(cancellationToken);
            await auditService.LogAsync("Logout", nameof(User), token.UserId.ToString(), null, null, cancellationToken);
        }
    }
}
