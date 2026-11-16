using System.Globalization;
using System.Text;
using BehaAtel.Application.Common.Models;

namespace BehaAtel.Application.Common.Services;

public static class TelegramMessageFormatter
{
    private const int MaximumBodyLength = 3500;
    private const int MaximumMessageLength = 4096;

    public static string Format(
        TelegramMessageKind kind,
        string title,
        string body,
        TelegramAudience audience,
        string author,
        DateTime createdAt)
    {
        var type = kind == TelegramMessageKind.Announcement ? "Объявление" : "Уведомление";
        var header = new StringBuilder()
            .Append("📢 <b>").Append(EscapeHtml(type)).Append("</b>\n")
            .Append("<b>Заголовок:</b> ").Append(EscapeHtml(title)).Append('\n')
            .Append("<b>Аудитория:</b> ").Append(FormatAudience(audience)).Append('\n')
            .Append("<b>Автор:</b> ").Append(EscapeHtml(author)).Append('\n')
            .Append("<b>Время:</b> ")
            .Append(DateTime.SpecifyKind(createdAt, DateTimeKind.Utc)
                .ToString("yyyy-MM-dd HH:mm 'UTC'", CultureInfo.InvariantCulture))
            .Append("\n\n<b>Текст:</b> ")
            .ToString();

        var escapedBody = EscapeHtml(TruncateBody(body));
        var availableBodyLength = Math.Max(0, MaximumMessageLength - header.Length);
        escapedBody = TruncateEscapedText(escapedBody, Math.Min(MaximumBodyLength, availableBodyLength));
        return header + escapedBody;
    }

    private static string FormatAudience(TelegramAudience audience) =>
        audience.Kind switch
        {
            TelegramAudienceKind.AllResidents => "Всем жильцам",
            TelegramAudienceKind.Building => $"Здание {EscapeHtml(audience.BuildingName ?? string.Empty)}",
            TelegramAudienceKind.SelectedResidents => FormatSelectedAudience(audience),
            _ => throw new ArgumentOutOfRangeException(nameof(audience))
        };

    private static string FormatSelectedAudience(TelegramAudience audience)
    {
        var names = audience.ResidentNames?.Take(10).ToArray() ?? [];
        var details = names.Length == 0 ? string.Empty : $" ({string.Join(", ", names.Select(EscapeHtml))}";
        var additionalCount = Math.Max(0, audience.RecipientCount - names.Length);
        if (additionalCount > 0)
        {
            details += names.Length == 0 ? $" (и ещё {additionalCount}" : $" и ещё {additionalCount}";
        }

        if (details.Length > 0)
        {
            details += ")";
        }

        return $"Выбранным: {audience.RecipientCount} жильцов{details}";
    }

    private static string TruncateBody(string value)
    {
        if (value.Length <= MaximumBodyLength)
        {
            return value;
        }

        var length = MaximumBodyLength;
        if (char.IsHighSurrogate(value[length - 1]))
        {
            length--;
        }

        return value[..length];
    }

    private static string TruncateEscapedText(string value, int maximumLength)
    {
        if (value.Length <= maximumLength)
        {
            return value;
        }

        var end = maximumLength;
        var ampersand = value.LastIndexOf('&', end - 1, end);
        if (ampersand >= 0 && value.IndexOf(';', ampersand, end - ampersand) < 0)
        {
            end = ampersand;
        }

        if (end > 0 && char.IsHighSurrogate(value[end - 1]))
        {
            end--;
        }

        return value[..end];
    }

    private static string EscapeHtml(string value) =>
        value.Replace("&", "&amp;", StringComparison.Ordinal)
            .Replace("<", "&lt;", StringComparison.Ordinal)
            .Replace(">", "&gt;", StringComparison.Ordinal)
            .Replace("\"", "&quot;", StringComparison.Ordinal)
            .Replace("'", "&#39;", StringComparison.Ordinal);
}
