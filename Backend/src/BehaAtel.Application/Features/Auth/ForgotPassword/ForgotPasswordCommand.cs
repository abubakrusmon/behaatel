using BehaAtel.Application.Features.Auth.Register;
using MediatR;

namespace BehaAtel.Application.Features.Auth.ForgotPassword;

public sealed record ForgotPasswordCommand(string Email) : IRequest<AuthActionDto>;
