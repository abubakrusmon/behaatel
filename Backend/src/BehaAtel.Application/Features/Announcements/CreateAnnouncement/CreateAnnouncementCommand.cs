using BehaAtel.Application.Features.Announcements.Common;
using BehaAtel.Domain.Enums;
using MediatR;

namespace BehaAtel.Application.Features.Announcements.CreateAnnouncement;

public sealed record CreateAnnouncementCommand(
    string Title,
    string Body,
    bool PublishNow,
    bool SendTelegram = true,
    AnnouncementAudience AudienceKind = AnnouncementAudience.AllResidents,
    Guid? BuildingId = null,
    IReadOnlyCollection<Guid>? ResidentIds = null)
    : IRequest<AnnouncementDto>;
