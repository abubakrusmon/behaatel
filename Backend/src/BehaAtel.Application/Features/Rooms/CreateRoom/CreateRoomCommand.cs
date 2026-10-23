using BehaAtel.Application.Features.Rooms.Common;
using BehaAtel.Domain.Enums;
using MediatR;

namespace BehaAtel.Application.Features.Rooms.CreateRoom;

public sealed record CreateRoomCommand(
    Guid BuildingId,
    string Number,
    int Floor,
    int Capacity,
    decimal PricePerBed,
    RoomType Type) : IRequest<RoomDto>;
