using BehaAtel.Application.Common.Interfaces;
using BehaAtel.Domain.Entities;
using BehaAtel.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BehaAtel.Application.Features.Auth.Common;

public static class EmailCodePolicy
{
    public static async Task<EmailCodeIssueResult> IssueAsync(
        IApplicationDbContext db,
        IEmailCodeService codeService,
        IEmailSender emailSender,
        IDateTimeProvider clock,
        User user,
        EmailCodePurpose purpose,
        CancellationToken cancellationToken)
    {
        var now = clock.UtcNow;
        var lastHour = now.AddHours(-1);
        var recent = await db.EmailCodes.Where(code => code.UserId == user.Id && code.CreatedAt > lastHour)
            .OrderByDescending(code => code.CreatedAt).ToListAsync(cancellationToken);
        var rateLimitReason = GetRateLimitReason(recent, now);
        if (rateLimitReason is not null)
            return new EmailCodeIssueResult(null, rateLimitReason);

        var rawCode = codeService.GenerateCode();
        var emailCode = EmailCode.Create(user.Id, purpose, codeService.HashCode(rawCode), now.AddMinutes(15), now);
        db.EmailCodes.Add(emailCode);
        var (subject, body) = purpose switch
        {
            EmailCodePurpose.EmailVerification => EmailTemplates.Verification(rawCode),
            EmailCodePurpose.PasswordReset => EmailTemplates.PasswordReset(rawCode),
            _ => EmailTemplates.LoginCode(rawCode)
        };
        await emailSender.SendAsync(user.Email, subject, body, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return new EmailCodeIssueResult(emailCode, null);
    }

    public static bool CanSendVerification(User user) =>
        user.Role == Role.User && !user.EmailConfirmed;

    public static bool CanRequestPasswordReset(User user) => user.EmailConfirmed;

    public static bool ShouldReopenRejectedRegistration(User user) =>
        user.Role == Role.User && user.ApprovalStatus == ApprovalStatus.Rejected;

    public static EmailCodeRateLimitReason? GetRateLimitReason(IReadOnlyCollection<EmailCode> recent, DateTime now)
    {
        if (recent.Count >= 5) return EmailCodeRateLimitReason.HourlyLimit;
        if (recent.Any(code => code.CreatedAt > now.AddSeconds(-60)))
            return EmailCodeRateLimitReason.Cooldown;
        return null;
    }

    public static void LogRateLimit(
        ILogger logger,
        User user,
        EmailCodePurpose purpose,
        EmailCodeRateLimitReason reason) =>
        logger.LogInformation(
            "Email code was not sent due to {RateLimitReason}. Recipient {Recipient}; purpose {Purpose}.",
            reason, MaskEmail(user.Email), purpose);

    public static string MaskEmail(string email)
    {
        var separator = email.IndexOf('@');
        if (separator <= 0) return "***";
        var visible = email[..Math.Min(2, separator)];
        return $"{visible}***{email[separator..]}";
    }
}

public sealed record EmailCodeIssueResult(EmailCode? Code, EmailCodeRateLimitReason? RateLimitReason);

public enum EmailCodeRateLimitReason
{
    Cooldown,
    HourlyLimit
}
