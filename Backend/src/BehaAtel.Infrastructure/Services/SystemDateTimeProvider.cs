using BehaAtel.Application.Common.Interfaces;

namespace BehaAtel.Infrastructure.Services;

public sealed class SystemDateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}
