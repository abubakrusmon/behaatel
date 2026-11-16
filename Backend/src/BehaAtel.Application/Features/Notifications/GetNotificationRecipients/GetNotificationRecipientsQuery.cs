using BehaAtel.Application.Features.Notifications.Common;
using MediatR;

namespace BehaAtel.Application.Features.Notifications.GetNotificationRecipients;

public sealed record GetNotificationRecipientsQuery : IRequest<IReadOnlyList<NotificationRecipientDto>>;
