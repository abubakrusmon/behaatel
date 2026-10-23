using BehaAtel.Application.Features.Auth.Register;
using MediatR;

namespace BehaAtel.Application.Features.Auth.VerifyEmail;

public sealed record VerifyEmailCommand(string Email, string Code) : IRequest<AuthActionDto>;
