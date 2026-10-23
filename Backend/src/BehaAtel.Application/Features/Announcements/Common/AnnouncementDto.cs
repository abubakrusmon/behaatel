namespace BehaAtel.Application.Features.Announcements.Common;

public sealed record AnnouncementDto(
    Guid Id,
    string Title,
    string Body,
    DateTime? PublishedAt,
    string AuthorName,
    DateTime CreatedAt);
