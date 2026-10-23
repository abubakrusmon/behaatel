using BehaAtel.Domain.Common;
using BehaAtel.Domain.Exceptions;

namespace BehaAtel.Domain.Entities;

public class RefreshToken : BaseEntity
{
    private RefreshToken()
    {
        Token = string.Empty;
    }

    private RefreshToken(Guid userId, string token, DateTime expiresAt, DateTime createdAt)
    {
        if (userId == Guid.Empty)
        {
            throw new DomainException("UserId не может быть пустым.");
        }

        if (string.IsNullOrWhiteSpace(token))
        {
            throw new DomainException("Токен не может быть пустым.");
        }

        EnsureUtc(expiresAt, nameof(expiresAt));
        EnsureUtc(createdAt, nameof(createdAt));
        if (expiresAt <= createdAt)
        {
            throw new DomainException("Срок действия токена должен быть позже даты создания.");
        }

        UserId = userId;
        Token = token.Trim();
        ExpiresAt = expiresAt;
        CreatedAt = createdAt;
    }

    public Guid UserId { get; private set; }
    public string Token { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? RevokedAt { get; private set; }
    public string? ReplacedByToken { get; private set; }
    public bool IsExpired => ExpiresAt <= DateTime.UtcNow;
    public bool IsRevoked => RevokedAt.HasValue;
    public bool IsActive => !IsExpired && !IsRevoked;

    public static RefreshToken Create(Guid userId, string token, DateTime expiresAt, DateTime? createdAt = null) =>
        new(userId, token, expiresAt, createdAt ?? DateTime.UtcNow);

    public void Revoke(string? replacedByToken = null)
    {
        if (IsRevoked)
        {
            throw new DomainException("Токен уже отозван.");
        }

        RevokedAt = DateTime.UtcNow;
        ReplacedByToken = string.IsNullOrWhiteSpace(replacedByToken) ? null : replacedByToken.Trim();
    }

    private static void EnsureUtc(DateTime value, string field)
    {
        if (value.Kind != DateTimeKind.Utc)
        {
            throw new DomainException($"{field} должен быть в UTC.");
        }
    }
}
