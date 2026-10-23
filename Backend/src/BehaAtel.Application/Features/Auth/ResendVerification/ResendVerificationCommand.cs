using BehaAtel.Application.Features.Auth.Register;
using MediatR;

namespace BehaAtel.Application.Features.Auth.ResendVerification;

public sealed record ResendVerificationCommand(string Email) : IRequest<AuthActionDto>;
