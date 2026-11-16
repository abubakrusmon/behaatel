using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using BehaAtel.Infrastructure.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BehaAtel.Infrastructure.Services;

public sealed class TelegramNotifier(
    HttpClient httpClient,
    IOptions<TelegramSettings> options,
    ILogger<TelegramNotifier> logger)
{
    private const int MaximumMessageLength = 4096;
    private static int _configurationWarningLogged;
    private readonly TelegramSettings _settings = options.Value;

    public Task SendAnnouncementAsync(
        string title,
        string body,
        IReadOnlyCollection<string> chatIds,
        CancellationToken ct)
    {
        if (chatIds.Count == 0)
            return Task.CompletedTask;

        var escapedTitle = EscapeHtml(title);
        var escapedBody = EscapeHtml(body);
        var prefix = $"📢 <b>{escapedTitle}</b>\n\n";
        return SendHtmlTextAsync(chatIds, prefix, escapedBody, ct);
    }

    private async Task SendHtmlTextAsync(
        IReadOnlyCollection<string> chatIds,
        string prefix,
        string text,
        CancellationToken ct)
    {
        if (!_settings.Enabled)
            return;

        if (string.IsNullOrWhiteSpace(_settings.Token))
        {
            if (Interlocked.Exchange(ref _configurationWarningLogged, 1) == 0)
                logger.LogWarning("Telegram не настроен");
            return;
        }

        foreach (var chatId in chatIds.Where(id => !string.IsNullOrWhiteSpace(id)).Distinct(StringComparer.Ordinal))
        {
            try
            {
                foreach (var chunk in SplitText(prefix, text))
                    await SendChunkAsync(chatId, chunk, ct);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception)
            {
                logger.LogError("Failed to send an announcement notification to a Telegram chat.");
            }
        }
    }

    private async Task SendChunkAsync(string chatId, string text, CancellationToken ct)
    {
        HttpResponseMessage response;
        try
        {
            response = await httpClient.PostAsJsonAsync(
                $"https://api.telegram.org/bot{_settings.Token}/sendMessage",
                new TelegramMessageRequest(chatId, text, "HTML"),
                ct);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception)
        {
            logger.LogError("Telegram request failed before receiving a response.");
            throw new InvalidOperationException("Telegram request failed.");
        }

        using (response)
        {
            var result = await ReadResultAsync(response, ct);
            if (response.StatusCode == (HttpStatusCode)429 && result.RetryAfterSeconds is { } retryAfter)
            {
                await Task.Delay(TimeSpan.FromSeconds(retryAfter), ct);
                await SendOnceAsync(chatId, text, ct);
                return;
            }

            if (!response.IsSuccessStatusCode || !result.Ok)
            {
                var description = SanitizeDescription(result.Description ?? "Telegram rejected the message.");
                logger.LogError(
                    "Telegram API request failed with HTTP status {StatusCode}: {Description}",
                    (int)response.StatusCode,
                    description);
                throw new InvalidOperationException(
                    $"Telegram API request failed with HTTP status {(int)response.StatusCode}: {description}");
            }
        }
    }

    private async Task SendOnceAsync(string chatId, string text, CancellationToken ct)
    {
        try
        {
            using var response = await httpClient.PostAsJsonAsync(
                $"https://api.telegram.org/bot{_settings.Token}/sendMessage",
                new TelegramMessageRequest(chatId, text, "HTML"),
                ct);
            var result = await ReadResultAsync(response, ct);
            if (!response.IsSuccessStatusCode || !result.Ok)
            {
                var description = SanitizeDescription(result.Description ?? "Telegram rejected the message.");
                logger.LogError(
                    "Telegram API retry failed with HTTP status {StatusCode}: {Description}",
                    (int)response.StatusCode,
                    description);
                throw new InvalidOperationException(
                    $"Telegram API request failed with HTTP status {(int)response.StatusCode}: {description}");
            }
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception)
        {
            logger.LogError("Telegram retry failed before receiving a response.");
            throw new InvalidOperationException("Telegram request failed.");
        }
    }

    private static async Task<TelegramResult> ReadResultAsync(HttpResponseMessage response, CancellationToken ct)
    {
        try
        {
            await using var stream = await response.Content.ReadAsStreamAsync(ct);
            using var document = await JsonDocument.ParseAsync(stream, cancellationToken: ct);
            var root = document.RootElement;
            var ok = root.TryGetProperty("ok", out var okElement) && okElement.ValueKind == JsonValueKind.True;
            var description = root.TryGetProperty("description", out var descriptionElement)
                ? descriptionElement.GetString()
                : null;
            int? retryAfterSeconds = null;
            if (root.TryGetProperty("parameters", out var parameters) &&
                parameters.TryGetProperty("retry_after", out var retryAfterElement) &&
                retryAfterElement.TryGetInt32(out var retryAfter))
            {
                retryAfterSeconds = Math.Max(0, retryAfter);
            }

            return new TelegramResult(ok, description, retryAfterSeconds);
        }
        catch (JsonException)
        {
            return new TelegramResult(false, null, null);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception)
        {
            return new TelegramResult(false, null, null);
        }
    }

    private static IReadOnlyList<string> SplitText(string prefix, string text)
    {
        var chunks = new List<string>();
        var textOffset = 0;
        var firstChunk = true;

        do
        {
            var chunkPrefix = firstChunk ? prefix : string.Empty;
            var availableLength = MaximumMessageLength - chunkPrefix.Length;
            var chunkLength = GetSafeChunkLength(text, textOffset, availableLength);
            chunks.Add(chunkPrefix + text.Substring(textOffset, chunkLength));
            textOffset += chunkLength;
            firstChunk = false;
        } while (textOffset < text.Length);

        return chunks;
    }

    private static int GetSafeChunkLength(string text, int offset, int maximumLength)
    {
        var length = Math.Min(maximumLength, text.Length - offset);
        if (length == 0)
            return 0;

        if (offset + length < text.Length && char.IsHighSurrogate(text[offset + length - 1]))
            length--;

        var ampersand = text.LastIndexOf('&', offset + length - 1, length);
        if (ampersand >= offset && text.IndexOf(';', ampersand, offset + length - ampersand) < 0)
            length = ampersand - offset;

        return length;
    }

    private static string EscapeHtml(string value) =>
        value.Replace("&", "&amp;", StringComparison.Ordinal)
            .Replace("<", "&lt;", StringComparison.Ordinal)
            .Replace(">", "&gt;", StringComparison.Ordinal);

    private string SanitizeDescription(string description)
    {
        if (!string.IsNullOrWhiteSpace(_settings.Token))
            description = description.Replace(_settings.Token, "[redacted]", StringComparison.Ordinal);

        return description.Replace("https://api.telegram.org/", "[Telegram API]/", StringComparison.OrdinalIgnoreCase);
    }

    private sealed record TelegramMessageRequest(
        [property: JsonPropertyName("chat_id")] string ChatId,
        [property: JsonPropertyName("text")] string Text,
        [property: JsonPropertyName("parse_mode")] string ParseMode);

    private sealed record TelegramResult(bool Ok, string? Description, int? RetryAfterSeconds);
}
