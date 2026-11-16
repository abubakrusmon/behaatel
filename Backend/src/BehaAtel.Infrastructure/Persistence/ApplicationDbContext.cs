using BehaAtel.Application.Common.Interfaces;
using BehaAtel.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BehaAtel.Infrastructure.Persistence;

public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) :
    DbContext(options),
    IApplicationDbContext
{
    public DbSet<User> Users => Set<User>();
    public DbSet<AdminPermission> AdminPermissions => Set<AdminPermission>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<EmailCode> EmailCodes => Set<EmailCode>();
    public DbSet<TelegramLinkAttempt> TelegramLinkAttempts => Set<TelegramLinkAttempt>();
    public DbSet<Resident> Residents => Set<Resident>();
    public DbSet<Building> Buildings => Set<Building>();
    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<Occupancy> Occupancies => Set<Occupancy>();
    public DbSet<Charge> Charges => Set<Charge>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<PaymentAllocation> PaymentAllocations => Set<PaymentAllocation>();
    public DbSet<Announcement> Announcements => Set<Announcement>();
    public DbSet<ResidentNotification> ResidentNotifications => Set<ResidentNotification>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<Setting> Settings => Set<Setting>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        modelBuilder.Entity<User>().HasQueryFilter(user => !user.IsDeleted);
        modelBuilder.Entity<Resident>().HasQueryFilter(resident => !resident.IsDeleted);
        modelBuilder.Entity<Building>().HasQueryFilter(building => !building.IsDeleted);
        modelBuilder.Entity<Room>().HasQueryFilter(room => !room.IsDeleted);
        modelBuilder.Entity<Announcement>().HasQueryFilter(announcement => !announcement.IsDeleted);
    }
}
