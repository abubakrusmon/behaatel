using System.Text.Json;
using BehaAtel.Application.Common.Interfaces;
using BehaAtel.Domain.Entities;

namespace BehaAtel.Infrastructure.Services;

public sealed class AuditService(
    IApplicationDbContext dbContext,
    ICurrentUser currentUser,
    IDateTimeProvider dateTimeProvider) : IAuditService
{
    public async Task LogAsync(
        string action,
        string entityName,
        string? entityId,
        object? oldValues,
        object? newValues,
        CancellationToken ct)
    {
        Guid? parsedEntityId = Guid.TryParse(entityId, out var id) ? id : null;
        var log = AuditLog.Create(
            currentUser.UserId,
            action,
            entityName,
            parsedEntityId,
            Serialize(oldValues),
            Serialize(newValues),
            currentUser.IpAddress,
            dateTimeProvider.UtcNow);

        dbContext.AuditLogs.Add(log);
        await dbContext.SaveChangesAsync(ct);
    }

    private static string? Serialize(object? value) =>
        value is null ? null : JsonSerializer.Serialize(value);
}
