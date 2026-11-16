using BehaAtel.Domain.Entities;
using BehaAtel.Infrastructure.Options;
using BehaAtel.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace BehaAtel.Application.UnitTests.Services;

public sealed class TelegramAccountLinkServiceTests
{
    [Fact]
    public async Task CreateChallengeAsync_WhenTelegramIsNotConfigured_ReturnsNoChallenge()
    {
        var logger = new Mock<ILogger<TelegramAccountLinkService>>();
        var service = new TelegramAccountLinkService(
            null!,
            null!,
            null!,
            null!,
            null!,
            Options.Create(new TelegramSettings()),
            logger.Object);
        var user = User.Create("admin@example.test", "password-hash", "Admin");

        var challenge = await service.CreateChallengeAsync(user, CancellationToken.None);

        Assert.False(service.IsConfigured);
        Assert.Null(challenge);
        logger.Verify(
            entry => entry.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((state, _) =>
                    state.ToString()!.Contains("Telegram account linking is not configured", StringComparison.Ordinal)),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
