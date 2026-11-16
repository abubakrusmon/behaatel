using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using BehaAtel.Application.Common.Interfaces;
using BehaAtel.Infrastructure.Options;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace BehaAtel.WebApi.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/v1/telegram")]
public sealed class TelegramWebhookController(
    ITelegramAccountLinkService linkService,
    IOptions<TelegramSettings> options) : ControllerBase
{
    [HttpPost("webhook")]
    public async Task<IActionResult> Webhook(JsonElement update, CancellationToken cancellationToken)
    {
        var configuredSecret = options.Value.WebhookSecret;
        var suppliedSecret = Request.Headers["X-Telegram-Bot-Api-Secret-Token"].ToString();
        if (string.IsNullOrWhiteSpace(configuredSecret) ||
            !CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(configuredSecret),
                Encoding.UTF8.GetBytes(suppliedSecret)))
            return Unauthorized();

        if (!update.TryGetProperty("message", out var message) ||
            !message.TryGetProperty("text", out var textElement) ||
            textElement.ValueKind != JsonValueKind.String ||
            !message.TryGetProperty("chat", out var chat) ||
            !chat.TryGetProperty("type", out var chatTypeElement) ||
            chatTypeElement.GetString() != "private" ||
            !chat.TryGetProperty("id", out var chatIdElement))
            return Ok();

        var text = textElement.GetString();
        var chatId = chatIdElement.ValueKind switch
        {
            JsonValueKind.Number => chatIdElement.GetRawText(),
            JsonValueKind.String => chatIdElement.GetString(),
            _ => null
        };
        var parts = text?.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts is not { Length: 2 } ||
            !string.Equals(parts[0].Split('@')[0], "/start", StringComparison.Ordinal) ||
            string.IsNullOrWhiteSpace(chatId))
            return Ok();

        await linkService.CompleteAsync(parts[1], chatId, cancellationToken);
        return Ok();
    }
}
