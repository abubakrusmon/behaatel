using BehaAtel.Application.Features.Auth.Register;
using MediatR;

namespace BehaAtel.Application.Features.Auth.ResetPassword;

public sealed record ResetPasswordCommand(string Email, string Code, string NewPassword) : IRequest<AuthActionDto>;
