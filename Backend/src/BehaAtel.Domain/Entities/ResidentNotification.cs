using BehaAtel.Domain.Common;
using BehaAtel.Domain.Exceptions;

namespace BehaAtel.Domain.Entities;

public sealed class ResidentNotification : BaseAuditableEntity
{
    private ResidentNotification()
    {
        Title = string.Empty;
        Body = string.Empty;
    }

    private ResidentNotification(Guid residentId, Guid authorId, string title, string body)
    {
        if (residentId == Guid.Empty || authorId == Guid.Empty)
        {
            throw new DomainException("Получатель и автор уведомления обязательны.");
        }

        if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(body))
        {
            throw new DomainException("Заголовок и текст уведомления обязательны.");
        }

        ResidentId = residentId;
        AuthorId = authorId;
        Title = title.Trim();
        Body = body.Trim();
    }

    public Guid ResidentId { get; private set; }
    public Guid AuthorId { get; private set; }
    public string Title { get; private set; }
    public string Body { get; private set; }

    public static ResidentNotification Create(Guid residentId, Guid authorId, string title, string body) =>
        new(residentId, authorId, title, body);
}
