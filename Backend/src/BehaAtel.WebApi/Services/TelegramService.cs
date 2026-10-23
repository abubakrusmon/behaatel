using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BehaAtel.WebApi.Services;

public sealed class TelegramService(HttpClient httpClient, IConfiguration configuration)
{
    private readonly string _token = configuration["Telegram:Token"]
        ?? throw new InvalidOperationException("Configuration key Telegram:Token is required.");
    private readonly string _chatId = configuration["Telegram:ChatId"]
        ?? throw new InvalidOperationException("Configuration key Telegram:ChatId is required.");

    public Task SendNotificationAsync(string message) =>
        SendMessageAsync(message, null);

    public Task SendConfirmationAsync(
        string text,
        string confirmCallbackData,
        string cancelCallbackData) =>
        SendMessageAsync(text, new InlineKeyboard(
        [
            [
                new InlineKeyboardButton("✅ Подтвердить", confirmCallbackData),
                new InlineKeyboardButton("❌ Отклонить", cancelCallbackData)
            ]
        ]));

    private async Task SendMessageAsync(string text, InlineKeyboard? keyboard)
    {
        var request = new TelegramMessageRequest(
            _chatId,
            text,
            "HTML",
            keyboard);
        using var response = await httpClient.PostAsJsonAsync(
            $"https://api.telegram.org/bot{_token}/sendMessage",
            request);

        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException(
                $"Telegram API request failed with HTTP {(int)response.StatusCode}.");

        await using var responseStream = await response.Content.ReadAsStreamAsync();
        using var responseJson = await JsonDocument.ParseAsync(responseStream);
        if (!responseJson.RootElement.TryGetProperty("ok", out var ok) || !ok.GetBoolean())
            throw new InvalidOperationException("Telegram API rejected the message.");
    }

    private sealed record TelegramMessageRequest(
        [property: JsonPropertyName("chat_id")] string ChatId,
        [property: JsonPropertyName("text")] string Text,
        [property: JsonPropertyName("parse_mode")] string ParseMode,
        [property: JsonPropertyName("reply_markup")] InlineKeyboard? ReplyMarkup);

    private sealed record InlineKeyboard(
        [property: JsonPropertyName("inline_keyboard")] InlineKeyboardButton[][] InlineKeyboardRows);

    private sealed record InlineKeyboardButton(
        [property: JsonPropertyName("text")] string Text,
        [property: JsonPropertyName("callback_data")] string CallbackData);
}
