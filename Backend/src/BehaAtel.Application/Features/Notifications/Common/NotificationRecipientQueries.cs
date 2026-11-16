using BehaAtel.Application.Common.Interfaces;
using BehaAtel.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace BehaAtel.Application.Features.Notifications.Common;

internal static class NotificationRecipientQueries
{
    public const string NoRoomLabel = "Без комнаты";

    /// <summary>
    /// Получатели: проживающие жильцы и одобренные жильцы, которых ещё не заселили в комнату.
    /// Выселенные (были заселены и съехали) не получают рассылку.
    /// </summary>
    public static IQueryable<ActiveNotificationRecipient> GetActive(IApplicationDbContext dbContext) =>
        from resident in dbContext.Residents.AsNoTracking()
            .Where(resident => !resident.IsDeleted &&
                               (resident.Status == ResidentStatus.Active ||
                                !dbContext.Occupancies.Any(occupancy => occupancy.ResidentId == resident.Id)))
        join user in dbContext.Users.AsNoTracking()
                .Where(user => !user.IsDeleted && user.IsActive && user.Role == Role.User)
            on resident.UserId equals user.Id
        join occupancy in dbContext.Occupancies.AsNoTracking().Where(occupancy => occupancy.To == null)
            on resident.Id equals occupancy.ResidentId into occupancies
        from occupancy in occupancies.DefaultIfEmpty()
        join room in dbContext.Rooms.AsNoTracking().Where(room => !room.IsDeleted)
            on occupancy.RoomId equals room.Id into rooms
        from room in rooms.DefaultIfEmpty()
        join building in dbContext.Buildings.AsNoTracking().Where(building => !building.IsDeleted)
            on room.BuildingId equals building.Id into buildings
        from building in buildings.DefaultIfEmpty()
        select new ActiveNotificationRecipient
        {
            ResidentId = resident.Id,
            FullName = user.FullName,
            BuildingId = building == null ? Guid.Empty : building.Id,
            BuildingName = building == null ? NoRoomLabel : building.Name,
            TelegramChatId = user.TelegramChatId
        };
}
