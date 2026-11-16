using BehaAtel.Domain.Common;

namespace BehaAtel.Domain.Entities;

public sealed class TelegramLinkAttempt : BaseAuditableEntity
{
    private TelegramLinkAttempt()
    {
        StartCodeHash = string.Empty;
        PollTokenHash = string.Empty;
    }

    private TelegramLinkAttempt(Guid userId, string startCodeHash, string pollTokenHash, DateTime expiresAt)
    {
        UserId = userId;
        StartCodeHash = startCodeHash;
        PollTokenHash = pollTokenHash;
        ExpiresAt = expiresAt;
    }

    public Guid UserId { get; private set; }
    public string StartCodeHash { get; private set; }
    public string PollTokenHash { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public DateTime? BoundAt { get; private set; }
    public DateTime? ConsumedAt { get; private set; }

    public static TelegramLinkAttempt Create(
        Guid userId,
        string startCodeHash,
        string pollTokenHash,
        DateTime expiresAt)
    {
        if (userId == Guid.Empty) throw new ArgumentException("User id is required.", nameof(userId));
        if (expiresAt.Kind != DateTimeKind.Utc) throw new ArgumentException("Expiry must be UTC.", nameof(expiresAt));
        return new TelegramLinkAttempt(userId, startCodeHash, pollTokenHash, expiresAt);
    }

    public void MarkBound(DateTime now)
    {
        if (now.Kind != DateTimeKind.Utc) throw new ArgumentException("Time must be UTC.", nameof(now));
        if (BoundAt.HasValue || ConsumedAt.HasValue || ExpiresAt <= now)
            throw new InvalidOperationException("Telegram link attempt is no longer usable.");
        BoundAt = now;
    }

    public void MarkConsumed(DateTime now)
    {
        if (now.Kind != DateTimeKind.Utc) throw new ArgumentException("Time must be UTC.", nameof(now));
        if (!BoundAt.HasValue || ConsumedAt.HasValue || ExpiresAt <= now)
            throw new InvalidOperationException("Telegram link attempt is no longer usable.");
        ConsumedAt = now;
    }
}
