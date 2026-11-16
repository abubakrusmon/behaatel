using BehaAtel.Application.Common.Interfaces;
using MediatR;

namespace BehaAtel.Application.Features.Announcements.PublishAnnouncement;

public sealed record PublishAnnouncementCommand(Guid Id, bool SendTelegram = true) : IRequest<TelegramQueueStatus?>;
