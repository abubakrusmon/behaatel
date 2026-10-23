using BehaAtel.Application.Features.Announcements.Common;
using MediatR;

namespace BehaAtel.Application.Features.Announcements.CreateAnnouncement;

public sealed record CreateAnnouncementCommand(string Title, string Body, bool PublishNow)
    : IRequest<AnnouncementDto>;
