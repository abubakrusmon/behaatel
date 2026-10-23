using BehaAtel.Application.Features.Auth.Common;
using MediatR;

namespace BehaAtel.Application.Features.Auth.Refresh;

public sealed record RefreshTokenCommand(string RefreshToken) : IRequest<AuthResponse>;
