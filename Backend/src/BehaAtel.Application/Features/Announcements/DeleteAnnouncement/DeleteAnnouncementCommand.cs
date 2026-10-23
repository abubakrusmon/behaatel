using MediatR;

namespace BehaAtel.Application.Features.Announcements.DeleteAnnouncement;

public sealed record DeleteAnnouncementCommand(Guid Id) : IRequest;
