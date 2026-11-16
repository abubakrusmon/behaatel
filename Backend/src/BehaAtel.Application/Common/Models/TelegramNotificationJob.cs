namespace BehaAtel.Application.Common.Models;

public sealed record TelegramRecipientMessage(string ChatId, string Text);

public sealed record TelegramNotificationJob(
    string AdminMessage,
    IReadOnlyCollection<TelegramRecipientMessage>? Recipients = null,
    int TotalRecipients = 0,
    int WithoutTelegram = 0,
    bool IncludeDeliverySummary = false);
