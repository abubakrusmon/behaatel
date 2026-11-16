using System.Security.Cryptography;
using System.Text;
using BehaAtel.Application.Common.Exceptions;
using BehaAtel.Application.Common.Interfaces;
using BehaAtel.Application.Features.Auth.Common;
using BehaAtel.Domain.Entities;
using BehaAtel.Domain.Enums;
using BehaAtel.Infrastructure.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BehaAtel.Infrastructure.Services;

public sealed class TelegramAccountLinkService(
    IApplicationDbContext db,
    IDateTimeProvider clock,
    IJwtTokenService jwt,
    ICurrentUser currentUser,
    IAuditService audit,
    IOptions<TelegramSettings> options,
    ILogger<TelegramAccountLinkService> logger,
    TelegramBotInfo? botInfo = null) : ITelegramAccountLinkService
{
    private static readonly TimeSpan AttemptLifetime = TimeSpan.FromMinutes(10);
    private static int _configurationWarningLogged;
    private readonly TelegramSettings _settings = options.Value;
    // Без WebhookSecret бот работает через long polling и узнаёт своё имя через getMe.
    private string? BotUsername =>
        string.IsNullOrWhiteSpace(_settings.BotUsername) ? botInfo?.Username : _settings.BotUsername;

    public bool IsConfigured =>
        _settings.Enabled &&
        !string.IsNullOrWhiteSpace(BotUsername) &&
        !string.IsNullOrWhiteSpace(_settings.Token);

    public async Task<TelegramLinkChallengeDto?> CreateChallengeAsync(
        User user,
        CancellationToken cancellationToken)
    {
        if (!user.RequiresTelegramLink)
            throw new InvalidOperationException("This account does not require Telegram linking.");
        if (!IsConfigured)
        {
            if (Interlocked.Exchange(ref _configurationWarningLogged, 1) == 0)
                logger.LogWarning("Telegram account linking is not configured; allowing login without a Telegram challenge.");
            return null;
        }

        var startCode = GenerateToken();
        var pollToken = GenerateToken();
        var expiresAt = clock.UtcNow.Add(AttemptLifetime);
        var attempt = TelegramLinkAttempt.Create(
            user.Id,
            Hash(startCode),
            Hash(pollToken),
            expiresAt);
        db.TelegramLinkAttempts.Add(attempt);
        await db.SaveChangesAsync(cancellationToken);

        var username = BotUsername!.Trim().TrimStart('@');
        return new TelegramLinkChallengeDto(
            attempt.Id,
            pollToken,
            $"https://t.me/{Uri.EscapeDataString(username)}?start={Uri.EscapeDataString(startCode)}",
            expiresAt);
    }

    public async Task<bool> CompleteAsync(
        string startCode,
        string chatId,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(startCode) || string.IsNullOrWhiteSpace(chatId))
            return false;

        var now = clock.UtcNow;
        var attempt = await db.TelegramLinkAttempts.SingleOrDefaultAsync(candidate =>
            candidate.StartCodeHash == Hash(startCode) &&
            candidate.BoundAt == null &&
            candidate.ConsumedAt == null &&
            candidate.ExpiresAt > now,
            cancellationToken);
        if (attempt is null) return false;

        var user = await db.Users.SingleOrDefaultAsync(candidate => candidate.Id == attempt.UserId, cancellationToken);
        if (user is null || !user.IsActive || user.IsDeleted ||
            user.ApprovalStatus != ApprovalStatus.Approved || !user.EmailConfirmed ||
            !user.RequiresTelegramLink ||
            await db.Users.AnyAsync(candidate =>
                candidate.TelegramChatId == chatId && candidate.Id != user.Id,
                cancellationToken))
            return false;

        try
        {
            user.LinkTelegram(chatId);
            attempt.MarkBound(now);
            await db.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (DbUpdateException)
        {
            return false;
        }
    }

    public async Task<TelegramLinkPollDto> PollAsync(
        Guid attemptId,
        string pollToken,
        CancellationToken cancellationToken)
    {
        var attempt = await db.TelegramLinkAttempts.SingleOrDefaultAsync(
            candidate => candidate.Id == attemptId,
            cancellationToken);
        if (attempt is null ||
            !FixedTimeHashEquals(attempt.PollTokenHash, pollToken) ||
            attempt.ExpiresAt <= clock.UtcNow ||
            attempt.ConsumedAt.HasValue)
            return new TelegramLinkPollDto(false, true, null);

        if (!attempt.BoundAt.HasValue)
            return new TelegramLinkPollDto(false, false, null);

        var user = await db.Users.Include(candidate => candidate.Permissions)
            .SingleOrDefaultAsync(candidate => candidate.Id == attempt.UserId, cancellationToken);
        if (user is null || !user.IsActive || user.IsDeleted ||
            user.ApprovalStatus != ApprovalStatus.Approved || !user.EmailConfirmed ||
            user.RequiresTelegramLink)
            return new TelegramLinkPollDto(false, true, null);

        attempt.MarkConsumed(clock.UtcNow);
        user.RegisterLogin();
        var refreshToken = jwt.GenerateRefreshToken(user.Id, currentUser.IpAddress);
        db.RefreshTokens.Add(refreshToken);
        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return new TelegramLinkPollDto(false, true, null);
        }

        await audit.LogAsync("Login", nameof(User), user.Id.ToString(), null,
            new { user.Email, TelegramLinked = true }, cancellationToken);
        var session = new AuthResponse(
            jwt.GenerateAccessToken(user),
            refreshToken.Token,
            refreshToken.ExpiresAt,
            UserInfoDto.FromUser(user));
        return new TelegramLinkPollDto(true, false, session);
    }

    private static string GenerateToken() =>
        Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');

    private static string Hash(string value) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)));

    private static bool FixedTimeHashEquals(string expectedHash, string value)
    {
        if (string.IsNullOrEmpty(value)) return false;
        var candidate = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        byte[] expected;
        try
        {
            expected = Convert.FromHexString(expectedHash);
        }
        catch (FormatException)
        {
            return false;
        }

        return candidate.Length == expected.Length &&
               CryptographicOperations.FixedTimeEquals(candidate, expected);
    }
}
