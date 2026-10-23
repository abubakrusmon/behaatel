namespace BehaAtel.Application.Common.Interfaces;

public interface IAuditService
{
    Task LogAsync(string action, string entityName, string? entityId, object? oldValues, object? newValues, CancellationToken ct);
}
