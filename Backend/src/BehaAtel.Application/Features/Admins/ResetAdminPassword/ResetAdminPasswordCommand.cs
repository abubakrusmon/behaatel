using MediatR;

namespace BehaAtel.Application.Features.Admins.ResetAdminPassword;

public sealed record ResetAdminPasswordCommand(Guid AdminId, string NewPassword) : IRequest;
