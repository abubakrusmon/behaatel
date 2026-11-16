namespace BehaAtel.Application.Features.Notifications.Common;

public sealed record NotificationRecipientDto(
    Guid ResidentId,
    string FullName,
    Guid BuildingId,
    string BuildingName,
    bool TelegramConnected);

// Свойства (а не параметры конструктора), чтобы EF мог транслировать Where/OrderBy по полям проекции.
internal sealed record ActiveNotificationRecipient
{
    public Guid ResidentId { get; init; }
    public string FullName { get; init; } = string.Empty;
    public Guid BuildingId { get; init; }
    public string BuildingName { get; init; } = string.Empty;
    public string? TelegramChatId { get; init; }
}
