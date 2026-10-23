using BehaAtel.Domain.Common;
using BehaAtel.Domain.Exceptions;

namespace BehaAtel.Domain.Entities;

public class Announcement : BaseAuditableEntity, ISoftDelete
{
    private Announcement()
    {
        Title = string.Empty;
        Body = string.Empty;
    }

    private Announcement(string title, string body, Guid authorId)
    {
        if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(body))
        {
            throw new DomainException("Заголовок и текст объявления обязательны.");
        }

        if (authorId == Guid.Empty)
        {
            throw new DomainException("AuthorId не может быть пустым.");
        }

        Title = title.Trim();
        Body = body.Trim();
        AuthorId = authorId;
    }

    public string Title { get; private set; }
    public string Body { get; private set; }
    public DateTime? PublishedAt { get; private set; }
    public Guid AuthorId { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAt { get; private set; }

    public static Announcement Create(string title, string body, Guid authorId) =>
        new(title, body, authorId);

    public void Update(string title, string body)
    {
        if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(body))
        {
            throw new DomainException("Заголовок и текст объявления обязательны.");
        }

        Title = title.Trim();
        Body = body.Trim();
    }

    public void Publish()
    {
        Publish(DateTime.UtcNow);
    }

    public void Publish(DateTime publishedAt)
    {
        if (PublishedAt.HasValue)
        {
            throw new DomainException("Объявление уже опубликовано.");
        }

        if (publishedAt.Kind != DateTimeKind.Utc)
        {
            throw new DomainException("Дата публикации должна быть в UTC.");
        }

        PublishedAt = publishedAt;
    }

    public void MarkDeleted()
    {
        MarkDeleted(DateTime.UtcNow);
    }

    public void MarkDeleted(DateTime deletedAt)
    {
        if (deletedAt.Kind != DateTimeKind.Utc)
        {
            throw new DomainException("Дата удаления должна быть в UTC.");
        }

        IsDeleted = true;
        DeletedAt = deletedAt;
    }
}
