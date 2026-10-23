using BehaAtel.Application.Features.Rooms.Common;
using MediatR;

namespace BehaAtel.Application.Features.Rooms.GetRoomById;

public sealed record GetRoomByIdQuery(Guid Id) : IRequest<RoomDetailsDto>;
