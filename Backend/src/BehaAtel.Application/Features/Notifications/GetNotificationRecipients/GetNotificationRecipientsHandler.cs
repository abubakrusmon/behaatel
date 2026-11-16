using BehaAtel.Application.Common.Interfaces;
using BehaAtel.Application.Features.Notifications.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BehaAtel.Application.Features.Notifications.GetNotificationRecipients;

public sealed class GetNotificationRecipientsHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetNotificationRecipientsQuery, IReadOnlyList<NotificationRecipientDto>>
{
    public async Task<IReadOnlyList<NotificationRecipientDto>> Handle(
        GetNotificationRecipientsQuery request,
        CancellationToken cancellationToken) =>
        await NotificationRecipientQueries.GetActive(dbContext)
            .OrderBy(recipient => recipient.FullName)
            .Select(recipient => new NotificationRecipientDto(
                recipient.ResidentId,
                recipient.FullName,
                recipient.BuildingId,
                recipient.BuildingName,
                recipient.TelegramChatId != null))
            .ToListAsync(cancellationToken);
}
