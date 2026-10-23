using MediatR;

namespace BehaAtel.Application.Features.Me.GetMyRoom;

public sealed record GetMyRoomQuery : IRequest<MyRoomDto?>;
