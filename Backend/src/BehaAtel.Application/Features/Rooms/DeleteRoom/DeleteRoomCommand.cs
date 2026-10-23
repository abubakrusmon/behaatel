using MediatR;

namespace BehaAtel.Application.Features.Rooms.DeleteRoom;

public sealed record DeleteRoomCommand(Guid Id) : IRequest;
