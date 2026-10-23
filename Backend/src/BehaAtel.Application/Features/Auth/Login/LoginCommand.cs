using BehaAtel.Application.Features.Auth.Common;
using MediatR;

namespace BehaAtel.Application.Features.Auth.Login;

public sealed record LoginCommand(string Email, string Password) : IRequest<LoginResultDto>;
