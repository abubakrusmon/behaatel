using BehaAtel.Application.Common.Interfaces;
using BehaAtel.Domain.Common;
using BehaAtel.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace BehaAtel.Infrastructure.Persistence;

public sealed class AuditableEntityInterceptor(
    ICurrentUser currentUser,
    IDateTimeProvider dateTimeProvider) : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        StampEntries(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        StampEntries(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void StampEntries(DbContext? context)
    {
        if (context is null)
        {
            return;
        }

        var now = dateTimeProvider.UtcNow;
        var userId = currentUser.UserId;

        foreach (var entry in context.ChangeTracker.Entries<BaseAuditableEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Property(nameof(BaseAuditableEntity.CreatedAt)).CurrentValue = now;
                entry.Property(nameof(BaseAuditableEntity.CreatedBy)).CurrentValue = userId;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Property(nameof(BaseAuditableEntity.UpdatedAt)).CurrentValue = now;
                entry.Property(nameof(BaseAuditableEntity.UpdatedBy)).CurrentValue = userId;
            }
        }

        foreach (var entry in context.ChangeTracker.Entries<User>())
        {
            if (entry.State is EntityState.Added or EntityState.Modified)
            {
                var emailProperty = entry.Property(user => user.Email);
                emailProperty.CurrentValue = emailProperty.CurrentValue.Trim().ToLowerInvariant();
            }
        }
    }
}
