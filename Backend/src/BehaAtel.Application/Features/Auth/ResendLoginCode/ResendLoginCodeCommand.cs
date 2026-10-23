using BehaAtel.Application.Features.Auth.Register;
using MediatR;

namespace BehaAtel.Application.Features.Auth.ResendLoginCode;

public sealed record ResendLoginCodeCommand(Guid ChallengeId) : IRequest<AuthActionDto>;
