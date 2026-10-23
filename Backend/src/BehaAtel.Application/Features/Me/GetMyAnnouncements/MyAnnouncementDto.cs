namespace BehaAtel.Application.Features.Me.GetMyAnnouncements;

public sealed record MyAnnouncementDto(
    Guid Id,
    string Title,
    string Body,
    DateTime PublishedAt);
