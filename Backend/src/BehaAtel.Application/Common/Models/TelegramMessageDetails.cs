namespace BehaAtel.Application.Common.Models;

public enum TelegramMessageKind
{
    Announcement,
    Notification
}

public enum TelegramAudienceKind
{
    AllResidents,
    Building,
    SelectedResidents
}

public sealed record TelegramAudience(
    TelegramAudienceKind Kind,
    int RecipientCount,
    string? BuildingName = null,
    IReadOnlyCollection<string>? ResidentNames = null);
