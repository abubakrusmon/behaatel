using System.Threading.Channels;
using BehaAtel.Application.Common.Interfaces;

namespace BehaAtel.Infrastructure.Services;

public sealed record QueuedEmail(string To, string Subject, string HtmlBody);

/// <summary>Кладёт письмо в очередь и сразу возвращает управление — SMTP не тормозит HTTP-запрос.</summary>
public sealed class QueuedEmailSender(Channel<QueuedEmail> channel) : IEmailSender
{
    public Task SendAsync(string to, string subject, string htmlBody, CancellationToken cancellationToken)
    {
        if (!channel.Writer.TryWrite(new QueuedEmail(to, subject, htmlBody)))
        {
            throw new InvalidOperationException("Не удалось поставить письмо в очередь.");
        }

        return Task.CompletedTask;
    }
}
