using BehaAtel.Application.Features.Auth.Common;
using BehaAtel.Domain.Entities;

namespace BehaAtel.Application.Common.Interfaces;

public interface ITelegramAccountLinkService
{
    bool IsConfigured { get; }
    Task<TelegramLinkChallengeDto?> CreateChallengeAsync(User user, CancellationToken cancellationToken);
    Task<bool> CompleteAsync(string startCode, string chatId, CancellationToken cancellationToken);
    Task<TelegramLinkPollDto> PollAsync(
        Guid attemptId,
        string pollToken,
        CancellationToken cancellationToken);
}
