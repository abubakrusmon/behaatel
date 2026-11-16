namespace BehaAtel.Infrastructure.Options;

public sealed class TelegramSettings
{
    public const string SectionName = "Telegram";

    public string? Token { get; set; }

    public string? ChatId { get; set; }

    public string? BotUsername { get; set; }

    public string? WebhookSecret { get; set; }

    public bool Enabled { get; set; } = true;
}
