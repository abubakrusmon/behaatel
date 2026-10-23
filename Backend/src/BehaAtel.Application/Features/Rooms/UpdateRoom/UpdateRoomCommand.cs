using BehaAtel.Application.Features.Rooms.Common;
using BehaAtel.Domain.Enums;
using MediatR;

namespace BehaAtel.Application.Features.Rooms.UpdateRoom;

public sealed record UpdateRoomCommand(
    Guid Id,
    string Number,
    int Floor,
    int Capacity,
    decimal PricePerBed,
    RoomType Type) : IRequest<RoomDto>;
