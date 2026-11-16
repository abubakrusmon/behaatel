using BehaAtel.Application.Common.Interfaces;
using BehaAtel.Domain.Enums;

namespace BehaAtel.Application.Features.Announcements.Common;

public sealed record AnnouncementDto(
    Guid Id,
    string Title,
    string Body,
    DateTime? PublishedAt,
    string AuthorName,
    DateTime CreatedAt,
    AnnouncementAudience AudienceKind = AnnouncementAudience.AllResidents,
    Guid? BuildingId = null,
    Guid[]? ResidentIds = null,
    TelegramQueueStatus? TelegramStatus = null);
