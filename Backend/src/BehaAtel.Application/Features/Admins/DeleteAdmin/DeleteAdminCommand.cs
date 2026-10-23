using MediatR;

namespace BehaAtel.Application.Features.Admins.DeleteAdmin;

public sealed record DeleteAdminCommand(Guid AdminId) : IRequest;
