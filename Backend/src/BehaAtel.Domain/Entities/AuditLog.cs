using BehaAtel.Domain.Common;
using BehaAtel.Domain.Exceptions;

namespace BehaAtel.Domain.Entities;

public class AuditLog : BaseEntity
{
    private AuditLog()
    {
        Action = string.Empty;
        EntityName = string.Empty;
    }

    private AuditLog(Guid? userId, string action, string entityName, Guid? entityId, string? oldValues, string? newValues, string? ipAddress, DateTime createdAt)
    {
        if (string.IsNullOrWhiteSpace(action) || string.IsNullOrWhiteSpace(entityName))
        {
            throw new DomainException("Действие и название сущности обязательны.");
        }

        if (createdAt.Kind != DateTimeKind.Utc)
        {
            throw new DomainException("CreatedAt должен быть в UTC.");
        }

        UserId = userId;
        Action = action.Trim();
        EntityName = entityName.Trim();
        EntityId = entityId;
        OldValues = oldValues;
        NewValues = newValues;
        IpAddress = string.IsNullOrWhiteSpace(ipAddress) ? null : ipAddress.Trim();
        CreatedAt = createdAt;
    }

    public Guid? UserId { get; private set; }
    public string Action { get; private set; }
    public string EntityName { get; private set; }
    public Guid? EntityId { get; private set; }
    public string? OldValues { get; private set; }
    public string? NewValues { get; private set; }
    public string? IpAddress { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public static AuditLog Create(Guid? userId, string action, string entityName, Guid? entityId = null, string? oldValues = null, string? newValues = null, string? ipAddress = null, DateTime? createdAt = null) =>
        new(userId, action, entityName, entityId, oldValues, newValues, ipAddress, createdAt ?? DateTime.UtcNow);
}
