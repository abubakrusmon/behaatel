using BehaAtel.Domain.Enums;

namespace BehaAtel.Application.Features.Me.GetMyRoom;

public sealed record MyRoomDto(
    string BuildingName,
    string Address,
    int Floor,
    string RoomNumber,
    RoomType Type,
    IReadOnlyCollection<string> Neighbors);
