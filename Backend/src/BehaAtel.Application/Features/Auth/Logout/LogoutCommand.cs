using MediatR;

namespace BehaAtel.Application.Features.Auth.Logout;

public sealed record LogoutCommand(string RefreshToken) : IRequest;
