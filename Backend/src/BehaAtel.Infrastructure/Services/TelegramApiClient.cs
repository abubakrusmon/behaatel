using System.IO;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using BehaAtel.Infrastructure.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BehaAtel.Infrastructure.Services;

public sealed class TelegramApiClient(
    HttpClient httpClient,
    IOptions<TelegramSettings> options,
    ILogger<TelegramApiClient> logger)
{
    private const int MaximumAttempts = 5;
    private readonly TelegramSettings _settings = options.Value;

    public async Task<bool> SendAsync(
        string chatId,
        string text,
        CancellationToken cancellationToken)
    {
        for (var attempt = 1; attempt <= MaximumAttempts; attempt++)
        {
            HttpResponseMessage? response = null;
            try
            {
                response = await httpClient.PostAsJsonAsync(
                    $"https://api.telegram.org/bot{_settings.Token}/sendMessage",
                    new TelegramMessageRequest(chatId, text, "HTML"),
                    cancellationToken);
                var result = await ReadResultAsync(response, cancellationToken);

                if (response.IsSuccessStatusCode && result.Ok)
                {
                    return true;
                }

                if (attempt == MaximumAttempts)
                {
                    logger.LogError(
                        "Telegram delivery failed after {Attempts} attempts with HTTP status {StatusCode}: {Description}",
                        attempt,
                        (int)response.StatusCode,
                        SanitizeDescription(result.Description));
                    return false;
                }

                var delay = response.StatusCode == (HttpStatusCode)429 && result.RetryAfterSeconds is { } retryAfter
                    ? TimeSpan.FromSeconds(Math.Max(0, retryAfter))
                    : Backoff(attempt);
                await Task.Delay(delay, cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (HttpRequestException exception)
            {
                if (attempt == MaximumAttempts)
                {
                    logger.LogError(
                        "Telegram delivery failed after {Attempts} attempts ({ErrorType}).",
                        attempt,
                        exception.GetType().Name);
                    return false;
                }

                await Task.Delay(Backoff(attempt), cancellationToken);
            }
            catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                if (attempt == MaximumAttempts)
                {
                    logger.LogError("Telegram delivery timed out after {Attempts} attempts.", attempt);
                    return false;
                }

                await Task.Delay(Backoff(attempt), cancellationToken);
            }
            catch (IOException exception)
            {
                if (attempt == MaximumAttempts)
                {
                    logger.LogError(
                        "Telegram delivery failed after {Attempts} attempts ({ErrorType}).",
                        attempt,
                        exception.GetType().Name);
                    return false;
                }

                await Task.Delay(Backoff(attempt), cancellationToken);
            }
            finally
            {
                response?.Dispose();
            }
        }

        return false;
    }

    private static async Task<TelegramResult> ReadResultAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        try
        {
            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
            var root = document.RootElement;
            var ok = root.TryGetProperty("ok", out var okElement) && okElement.ValueKind == JsonValueKind.True;
            var description = root.TryGetProperty("description", out var descriptionElement)
                ? descriptionElement.GetString()
                : null;
            int? retryAfter = null;
            if (root.TryGetProperty("parameters", out var parameters) &&
                parameters.TryGetProperty("retry_after", out var retryAfterElement) &&
                retryAfterElement.TryGetInt32(out var seconds))
            {
                retryAfter = seconds;
            }

            return new TelegramResult(ok, description, retryAfter);
        }
        catch (JsonException)
        {
            return new TelegramResult(false, null, null);
        }
    }

    private string SanitizeDescription(string? description)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            return "Telegram returned an invalid response.";
        }

        if (!string.IsNullOrWhiteSpace(_settings.Token))
        {
            description = description.Replace(_settings.Token, "[redacted]", StringComparison.Ordinal);
        }

        return description.Replace("https://api.telegram.org/", "[Telegram API]/", StringComparison.OrdinalIgnoreCase);
    }

    private static TimeSpan Backoff(int attempt) => TimeSpan.FromSeconds(Math.Min(10, Math.Pow(2, attempt - 1)));

    private sealed record TelegramMessageRequest(
        [property: JsonPropertyName("chat_id")] string ChatId,
        [property: JsonPropertyName("text")] string Text,
        [property: JsonPropertyName("parse_mode")] string ParseMode);

    private sealed record TelegramResult(bool Ok, string? Description, int? RetryAfterSeconds);
}
