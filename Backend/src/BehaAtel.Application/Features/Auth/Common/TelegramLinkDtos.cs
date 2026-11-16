namespace BehaAtel.Application.Features.Auth.Common;

public sealed record TelegramLinkChallengeDto(
    Guid AttemptId,
    string PollToken,
    string BotLink,
    DateTime ExpiresAt);

public sealed record TelegramLinkPollDto(bool Completed, bool Expired, AuthResponse? Session);
