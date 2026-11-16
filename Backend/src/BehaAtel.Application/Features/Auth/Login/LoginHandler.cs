using BehaAtel.Application.Common.Exceptions;
using BehaAtel.Application.Common.Interfaces;
using BehaAtel.Application.Features.Settings.Common;
using BehaAtel.Application.Features.Auth.Common;
using BehaAtel.Domain.Entities;
using BehaAtel.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BehaAtel.Application.Features.Auth.Login;

public sealed class LoginHandler(
    IApplicationDbContext dbContext,
    IPasswordHasher passwordHasher,
    IJwtTokenService jwtTokenService,
    ICurrentUser currentUser,
    IAuditService auditService,
    IEmailCodeService codeService,
    IEmailSender emailSender,
    IDateTimeProvider dateTimeProvider,
    ITelegramAccountLinkService telegramLinkService)
    : IRequestHandler<LoginCommand, LoginResultDto>
{
    public async Task<LoginResultDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLower();
        var user = await dbContext.Users
            .Include(candidate => candidate.Permissions)
            .SingleOrDefaultAsync(candidate => candidate.Email.ToLower() == email, cancellationToken);

        if (user is null || !passwordHasher.Verify(user.PasswordHash, request.Password))
        {
            if (user is not null)
            {
                user.RegisterFailedLogin();
                await dbContext.SaveChangesAsync(cancellationToken);
            }

            throw new UnauthorizedException();
        }

        LoginAccessPolicy.EnsureEligible(user);
        if (!user.IsActive || user.IsDeleted || (user.LockoutEnd.HasValue && user.LockoutEnd > DateTime.UtcNow))
        {
            throw new UnauthorizedException();
        }

        var settingKey = user.Role == Role.User
            ? SettingKeys.TwoFactorEmailResidents
            : SettingKeys.TwoFactorEmailAdmins;
        var twoFactorEnabled = bool.TryParse(
            await dbContext.Settings.Where(setting => setting.Key == settingKey)
                .Select(setting => setting.Value).SingleOrDefaultAsync(cancellationToken),
            out var enabled) && enabled;
        if (twoFactorEnabled)
        {
            var challenge = await EmailCodePolicy.IssueAsync(dbContext, codeService, emailSender, dateTimeProvider,
                user, EmailCodePurpose.LoginTwoFactor, cancellationToken);
            if (challenge.Code is null)
                throw new ConflictException("Не удалось отправить код. Повторите попытку позже.");
            return new LoginResultDto(true, null, challenge.Code.Id, challenge.Code.ExpiresAt,
                EmailCodePolicy.MaskEmail(user.Email));
        }

        if (user.RequiresTelegramLink)
        {
            var telegramLink = await telegramLinkService.CreateChallengeAsync(user, cancellationToken);
            if (telegramLink is not null)
                return new LoginResultDto(false, null, null, null, null, telegramLink);
        }

        return new LoginResultDto(false, await CreateSessionAsync(user, cancellationToken), null, null, null);
    }

    private async Task<AuthResponse> CreateSessionAsync(User user, CancellationToken cancellationToken)
    {
        user.RegisterLogin();
        var refreshToken = jwtTokenService.GenerateRefreshToken(user.Id, currentUser.IpAddress);
        dbContext.RefreshTokens.Add(refreshToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        await auditService.LogAsync("Login", nameof(User), user.Id.ToString(), null, new { user.Email }, cancellationToken);

        return new AuthResponse(
            jwtTokenService.GenerateAccessToken(user),
            refreshToken.Token,
            refreshToken.ExpiresAt,
            UserInfoDto.FromUser(user));
    }
}
