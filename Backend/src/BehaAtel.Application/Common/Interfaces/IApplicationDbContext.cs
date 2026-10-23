using BehaAtel.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BehaAtel.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<AdminPermission> AdminPermissions { get; }
    DbSet<RefreshToken> RefreshTokens { get; }
    DbSet<EmailCode> EmailCodes { get; }
    DbSet<Resident> Residents { get; }
    DbSet<Building> Buildings { get; }
    DbSet<Room> Rooms { get; }
    DbSet<Occupancy> Occupancies { get; }
    DbSet<Charge> Charges { get; }
    DbSet<Payment> Payments { get; }
    DbSet<PaymentAllocation> PaymentAllocations { get; }
    DbSet<Announcement> Announcements { get; }
    DbSet<AuditLog> AuditLogs { get; }
    DbSet<Setting> Settings { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
