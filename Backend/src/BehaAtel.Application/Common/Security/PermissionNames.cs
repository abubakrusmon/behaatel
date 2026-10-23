using BehaAtel.Domain.Enums;

namespace BehaAtel.Application.Common.Security;

public static class PermissionNames
{
    public static string ToClaimValue(Permission permission) => permission switch
    {
        Permission.ResidentsView => "Residents.View",
        Permission.ResidentsManage => "Residents.Manage",
        Permission.RoomsView => "Rooms.View",
        Permission.RoomsManage => "Rooms.Manage",
        Permission.ChargesView => "Charges.View",
        Permission.ChargesManage => "Charges.Manage",
        Permission.PaymentsView => "Payments.View",
        Permission.PaymentsManage => "Payments.Manage",
        Permission.ReportsView => "Reports.View",
        Permission.AnnouncementsManage => "Announcements.Manage",
        _ => throw new ArgumentOutOfRangeException(nameof(permission), permission, "Неизвестное разрешение.")
    };
}
