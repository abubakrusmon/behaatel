namespace BehaAtel.Application.Features.AuditLogs.Common;

public sealed record AuditLogDto(
    Guid Id,
    Guid? UserId,
    string? UserName,
    string Action,
    string EntityName,
    Guid? EntityId,
    string? OldValues,
    string? NewValues,
    string? IpAddress,
    DateTime CreatedAt);
