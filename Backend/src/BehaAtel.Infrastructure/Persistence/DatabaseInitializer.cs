using BehaAtel.Application.Common.Interfaces;
using BehaAtel.Application.Features.Settings.Common;
using BehaAtel.Domain.Entities;
using BehaAtel.Domain.Enums;
using BehaAtel.Infrastructure.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace BehaAtel.Infrastructure.Persistence;

public sealed class DatabaseInitializer(
    ApplicationDbContext dbContext,
    IPasswordHasher passwordHasher,
    IDateTimeProvider dateTimeProvider,
    IOptions<SeedAdminOptions> seedOptions)
{
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        var seedAdmin = seedOptions.Value;
        if (string.IsNullOrWhiteSpace(seedAdmin.Email) ||
            string.IsNullOrWhiteSpace(seedAdmin.Password) ||
            string.IsNullOrWhiteSpace(seedAdmin.FullName))
        {
            throw new InvalidOperationException("Настройки SeedAdmin должны содержать Email, Password и FullName.");
        }

        var settings = await dbContext.Settings
            .ToListAsync(cancellationToken);
        foreach (var key in SettingKeys.All)
        {
            if (settings.All(setting => setting.Key != key))
            {
                dbContext.Settings.Add(Setting.Create(
                    key,
                    SettingKeys.DefaultValue(key),
                    dateTimeProvider.UtcNow));
            }
        }

        var hasSuperAdmin = await dbContext.Users
            .IgnoreQueryFilters()
            .AnyAsync(user => user.Role == Role.SuperAdmin, cancellationToken);
        if (!hasSuperAdmin)
        {
            var user = User.Create(
                seedAdmin.Email.Trim().ToLowerInvariant(),
                passwordHasher.Hash(seedAdmin.Password),
                seedAdmin.FullName,
                Role.SuperAdmin,
                mustChangePassword: true);
            dbContext.Users.Add(user);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
