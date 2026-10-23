using BehaAtel.Application.Features.Auth.Common;
using MediatR;

namespace BehaAtel.Application.Features.Auth.VerifyLoginCode;

public sealed record VerifyLoginCodeCommand(Guid ChallengeId, string Code) : IRequest<AuthResponse>;
