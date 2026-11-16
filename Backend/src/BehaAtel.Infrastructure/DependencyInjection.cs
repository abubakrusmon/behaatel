using BehaAtel.Application.Common.Interfaces;
using BehaAtel.Infrastructure.BackgroundJobs;
using BehaAtel.Infrastructure.Options;
using BehaAtel.Infrastructure.Persistence;
using BehaAtel.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Channels;

namespace BehaAtel.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Не задана строка подключения ConnectionStrings:Default.");
        }
        connectionString = NormalizeConnectionString(connectionString);

        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.Configure<SeedAdminOptions>(configuration.GetSection(SeedAdminOptions.SectionName));
        services.Configure<TelegramSettings>(configuration.GetSection(TelegramSettings.SectionName));
        services.AddSingleton(Channel.CreateUnbounded<TelegramQueuedMessage>(
            new UnboundedChannelOptions { SingleReader = true, SingleWriter = false }));
        services.AddSingleton<TelegramMessageQueue>();
        services.AddSingleton<ITelegramNotifier>(serviceProvider =>
            serviceProvider.GetRequiredService<TelegramMessageQueue>());
        services.AddHttpClient("TelegramPolling", client =>
        {
            client.BaseAddress = new Uri("https://api.telegram.org/");
            client.Timeout = TimeSpan.FromSeconds(40);
        });
        services.AddHttpClient("TelegramApi", client =>
        {
            client.BaseAddress = new Uri("https://api.telegram.org/");
            client.Timeout = TimeSpan.FromSeconds(20);
        });
        services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();
        services.AddScoped<IPasswordHasher, InfrastructurePasswordHasher>();
        services.AddScoped<ITelegramAccountLinkService, TelegramAccountLinkService>();
        services.AddSingleton<IEmailCodeService, InfrastructureEmailCodeService>();
        services.AddSingleton<SmtpEmailSender>();
        services.AddSingleton(Channel.CreateUnbounded<QueuedEmail>(
            new UnboundedChannelOptions { SingleReader = true, SingleWriter = false }));
        services.AddSingleton<IEmailSender, QueuedEmailSender>();
        services.AddSingleton<TelegramBotInfo>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IAuditService, AuditService>();
        services.AddScoped<AuditableEntityInterceptor>();
        services.AddScoped<DatabaseInitializer>();
        services.AddHostedService<MonthlyChargeBackgroundService>();
        services.AddHostedService<EmailCodeCleanupBackgroundService>();
        services.AddHostedService<TelegramDeliveryBackgroundService>();
        services.AddHostedService<TelegramPollingBackgroundService>();
        services.AddHostedService<EmailDeliveryBackgroundService>();
        services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
        {
            options.UseNpgsql(connectionString);
            options.AddInterceptors(serviceProvider.GetRequiredService<AuditableEntityInterceptor>());
        });
        services.AddScoped<IApplicationDbContext>(serviceProvider =>
            serviceProvider.GetRequiredService<ApplicationDbContext>());

        return services;
    }

    // Хостинги (Render, Heroku) отдают строку вида postgres://user:pass@host:port/db,
    // а Npgsql ожидает формат Host=...;Database=... — приводим к нужному виду.
    private static string NormalizeConnectionString(string connectionString)
    {
        if (!connectionString.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase) &&
            !connectionString.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase))
        {
            return connectionString;
        }

        var uri = new Uri(connectionString);
        var userInfo = uri.UserInfo.Split(':', 2);
        return new Npgsql.NpgsqlConnectionStringBuilder
        {
            Host = uri.Host,
            Port = uri.Port > 0 ? uri.Port : 5432,
            Database = uri.AbsolutePath.TrimStart('/'),
            Username = Uri.UnescapeDataString(userInfo[0]),
            Password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : null,
            SslMode = Npgsql.SslMode.Prefer
        }.ConnectionString;
    }
}
