using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using BehaAtel.Application.Common.Interfaces;
using BehaAtel.Infrastructure.Options;
using BehaAtel.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BehaAtel.Infrastructure.BackgroundJobs;

/// <summary>
/// Принимает сообщения бота через long polling (getUpdates). Нужен, когда у сервера нет публичного адреса
/// для webhook: жилец нажимает «Старт» в боте, и его аккаунт привязывается к Telegram.
/// Включается только если WebhookSecret не задан.
/// </summary>
public sealed class TelegramPollingBackgroundService(
    IHttpClientFactory httpClientFactory,
    IServiceScopeFactory scopeFactory,
    TelegramBotInfo botInfo,
    IOptions<TelegramSettings> options,
    ILogger<TelegramPollingBackgroundService> logger) : BackgroundService
{
    private readonly TelegramSettings _settings = options.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_settings.Enabled || string.IsNullOrWhiteSpace(_settings.Token) ||
            !string.IsNullOrWhiteSpace(_settings.WebhookSecret))
        {
            return;
        }

        // Не блокируем запуск приложения.
        await Task.Yield();
        var client = httpClientFactory.CreateClient("TelegramPolling");
        var offset = 0L;
        var initialized = false;

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                if (!initialized)
                {
                    initialized = await InitializeAsync(client, stoppingToken);
                    if (!initialized)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
                        continue;
                    }
                }

                using var response = await client.PostAsJsonAsync(
                    $"https://api.telegram.org/bot{_settings.Token}/getUpdates",
                    new { offset, timeout = 25, allowed_updates = new[] { "message" } },
                    stoppingToken);
                using var document = await JsonDocument.ParseAsync(
                    await response.Content.ReadAsStreamAsync(stoppingToken), cancellationToken: stoppingToken);
                var root = document.RootElement;
                if (!root.TryGetProperty("ok", out var ok) || ok.ValueKind != JsonValueKind.True ||
                    !root.TryGetProperty("result", out var updates))
                {
                    logger.LogWarning("Telegram getUpdates returned an error (HTTP {StatusCode}).", (int)response.StatusCode);
                    // 409 — одновременно настроен webhook или запущен другой экземпляр.
                    initialized = (int)response.StatusCode != 409 && initialized;
                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                    continue;
                }

                foreach (var update in updates.EnumerateArray())
                {
                    if (update.TryGetProperty("update_id", out var id) && id.TryGetInt64(out var updateId))
                    {
                        offset = updateId + 1;
                    }

                    await HandleUpdateAsync(client, update, stoppingToken);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                logger.LogWarning("Telegram polling error ({ErrorType}: {Message}); retrying.", exception.GetType().Name, Redact(exception.Message));
                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }
    }

    private string Redact(string message) =>
        string.IsNullOrWhiteSpace(_settings.Token) ? message : message.Replace(_settings.Token, "[redacted]", StringComparison.Ordinal);

    private async Task<bool> InitializeAsync(HttpClient client, CancellationToken cancellationToken)
    {
        // Polling несовместим с установленным webhook.
        using (await client.PostAsJsonAsync(
                   $"https://api.telegram.org/bot{_settings.Token}/deleteWebhook", new { drop_pending_updates = false }, cancellationToken))
        {
        }

        using var me = await client.GetAsync($"https://api.telegram.org/bot{_settings.Token}/getMe", cancellationToken);
        using var document = await JsonDocument.ParseAsync(
            await me.Content.ReadAsStreamAsync(cancellationToken), cancellationToken: cancellationToken);
        if (!me.IsSuccessStatusCode ||
            !document.RootElement.TryGetProperty("result", out var result) ||
            !result.TryGetProperty("username", out var username) ||
            string.IsNullOrWhiteSpace(username.GetString()))
        {
            logger.LogError("Telegram getMe failed (HTTP {StatusCode}); проверьте Telegram:Token.", (int)me.StatusCode);
            return false;
        }

        botInfo.Username = username.GetString();
        logger.LogInformation("Telegram bot @{Username} подключён (polling).", botInfo.Username);
        return true;
    }

    private async Task HandleUpdateAsync(HttpClient client, JsonElement update, CancellationToken cancellationToken)
    {
        if (!update.TryGetProperty("message", out var message) ||
            !message.TryGetProperty("text", out var textElement) ||
            textElement.ValueKind != JsonValueKind.String ||
            !message.TryGetProperty("chat", out var chat) ||
            !chat.TryGetProperty("type", out var chatType) || chatType.GetString() != "private" ||
            !chat.TryGetProperty("id", out var chatIdElement))
        {
            return;
        }

        var chatId = chatIdElement.GetRawText().Trim('"');
        var parts = textElement.GetString()?.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts is not { Length: > 0 } || !string.Equals(parts[0].Split('@')[0], "/start", StringComparison.Ordinal))
        {
            return;
        }

        string reply;
        if (parts.Length == 2)
        {
            using var scope = scopeFactory.CreateScope();
            var linkService = scope.ServiceProvider.GetRequiredService<ITelegramAccountLinkService>();
            var linked = await linkService.CompleteAsync(parts[1], chatId, cancellationToken);
            reply = linked
                ? "✅ Telegram подключён. Вернитесь на сайт — вход продолжится автоматически. Теперь сюда будут приходить объявления."
                : "⚠️ Ссылка недействительна или устарела. Войдите на сайт заново и откройте бота по новой ссылке.";
        }
        else
        {
            reply = "👋 Это бот общежития BehaAtel. Чтобы подключить уведомления, войдите на сайт и откройте бота по ссылке со страницы входа.";
        }

        using var _ = await client.PostAsJsonAsync(
            $"https://api.telegram.org/bot{_settings.Token}/sendMessage",
            new TelegramReply(chatId, reply),
            cancellationToken);
    }

    private sealed record TelegramReply(
        [property: JsonPropertyName("chat_id")] string ChatId,
        [property: JsonPropertyName("text")] string Text);
}
