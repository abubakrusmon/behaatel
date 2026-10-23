using BehaAtel.Application.Features.Announcements.Common;
using MediatR;

namespace BehaAtel.Application.Features.Announcements.UpdateAnnouncement;

public sealed record UpdateAnnouncementCommand(Guid Id, string Title, string Body) : IRequest<AnnouncementDto>;
