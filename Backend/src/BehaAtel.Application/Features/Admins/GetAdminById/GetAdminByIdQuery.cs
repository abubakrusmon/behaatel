using BehaAtel.Application.Features.Admins.Common;
using MediatR;

namespace BehaAtel.Application.Features.Admins.GetAdminById;

public sealed record GetAdminByIdQuery(Guid AdminId) : IRequest<AdminDto>;
