using MediatR;

namespace BehaAtel.Application.Features.Announcements.PublishAnnouncement;

public sealed record PublishAnnouncementCommand(Guid Id) : IRequest;
