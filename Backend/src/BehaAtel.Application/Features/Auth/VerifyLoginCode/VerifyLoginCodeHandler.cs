using BehaAtel.Application.Common.Exceptions;
using BehaAtel.Application.Common.Interfaces;
using BehaAtel.Application.Features.Auth.Common;
using BehaAtel.Domain.Entities;
using BehaAtel.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BehaAtel.Application.Features.Auth.VerifyLoginCode;

public sealed class VerifyLoginCodeHandler(
    IApplicationDbContext db,
    IEmailCodeService codeService,
    IDateTimeProvider clock,
    IJwtTokenService jwt,
    ICurrentUser currentUser,
    IAuditService audit,
    ITelegramAccountLinkService telegramLinkService) : IRequestHandler<VerifyLoginCodeCommand, LoginResultDto>
{
    public async Task<LoginResultDto> Handle(VerifyLoginCodeCommand request, CancellationToken cancellationToken)
    {
        var code = await db.EmailCodes.SingleOrDefaultAsync(candidate =>
            candidate.Id == request.ChallengeId && candidate.Purpose == EmailCodePurpose.LoginTwoFactor,
            cancellationToken);
        var user = code is null ? null : await db.Users.Include(candidate => candidate.Permissions)
            .SingleOrDefaultAsync(candidate => candidate.Id == code.UserId, cancellationToken);
        if (code is null || user is null || !user.IsActive || user.ApprovalStatus != ApprovalStatus.Approved ||
            !user.EmailConfirmed || !codeService.VerifyCode(code, request.Code, clock.UtcNow))
        {
            if (code is not null) await db.SaveChangesAsync(cancellationToken);
            throw new ValidationException(new Dictionary<string, string[]>
            {
                ["code"] = ["Неверный или просроченный код."]
            });
        }

        if (user.RequiresTelegramLink)
        {
            var telegramLink = await telegramLinkService.CreateChallengeAsync(user, cancellationToken);
            if (telegramLink is not null)
                return new LoginResultDto(false, null, null, null, null, telegramLink);
        }

        user.RegisterLogin();
        var refreshToken = jwt.GenerateRefreshToken(user.Id, currentUser.IpAddress);
        db.RefreshTokens.Add(refreshToken);
        await db.SaveChangesAsync(cancellationToken);
        await audit.LogAsync("Login", nameof(User), user.Id.ToString(), null, new { user.Email }, cancellationToken);
        return new LoginResultDto(false,
            new AuthResponse(jwt.GenerateAccessToken(user), refreshToken.Token, refreshToken.ExpiresAt,
                UserInfoDto.FromUser(user)),
            null, null, null);
    }
}
