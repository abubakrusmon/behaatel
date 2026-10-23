using System.Security.Cryptography;
using BehaAtel.Domain.Common;
using BehaAtel.Domain.Enums;
using BehaAtel.Domain.Exceptions;

namespace BehaAtel.Domain.Entities;

public sealed class EmailCode : BaseEntity
{
    public const int MaximumAttempts = 5;

    private EmailCode()
    {
        CodeHash = string.Empty;
    }

    private EmailCode(Guid userId, EmailCodePurpose purpose, string codeHash, DateTime expiresAt, DateTime createdAt)
    {
        if (userId == Guid.Empty) throw new DomainException("UserId не может быть пустым.");
        if (!Enum.IsDefined(purpose)) throw new DomainException("Назначение кода неизвестно.");
        if (string.IsNullOrWhiteSpace(codeHash)) throw new DomainException("Хэш кода обязателен.");
        if (createdAt.Kind != DateTimeKind.Utc || expiresAt.Kind != DateTimeKind.Utc || expiresAt <= createdAt)
            throw new DomainException("Срок действия кода некорректен.");

        UserId = userId;
        Purpose = purpose;
        CodeHash = codeHash;
        CreatedAt = createdAt;
        ExpiresAt = expiresAt;
    }

    public Guid UserId { get; private set; }
    public EmailCodePurpose Purpose { get; private set; }
    public string CodeHash { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public int Attempts { get; private set; }
    public DateTime? ConsumedAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public bool IsUsable(DateTime utcNow) =>
        utcNow.Kind == DateTimeKind.Utc && ExpiresAt > utcNow && Attempts < MaximumAttempts && ConsumedAt is null;

    public static EmailCode Create(Guid userId, EmailCodePurpose purpose, string codeHash, DateTime expiresAt, DateTime? createdAt = null) =>
        new(userId, purpose, codeHash, expiresAt, createdAt ?? DateTime.UtcNow);

    public bool TryVerify(string codeHash, DateTime utcNow)
    {
        if (!IsUsable(utcNow)) return false;
        Attempts++;
        var expected = Convert.FromHexString(CodeHash);
        byte[] supplied;
        try { supplied = Convert.FromHexString(codeHash); }
        catch (FormatException) { return false; }

        if (!CryptographicOperations.FixedTimeEquals(expected, supplied)) return false;
        ConsumedAt = utcNow;
        return true;
    }

    public void Consume(DateTime utcNow)
    {
        if (!IsUsable(utcNow)) throw new DomainException("Код истёк или уже использован.");
        ConsumedAt = utcNow;
    }
}
