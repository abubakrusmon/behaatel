namespace BehaAtel.Infrastructure.Services;

/// <summary>Имя бота, полученное через getMe при старте (если не задано в конфигурации).</summary>
public sealed class TelegramBotInfo
{
    private volatile string? _username;

    public string? Username
    {
        get => _username;
        set => _username = value;
    }
}
