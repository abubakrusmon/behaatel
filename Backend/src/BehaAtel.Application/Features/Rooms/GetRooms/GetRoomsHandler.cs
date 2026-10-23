using BehaAtel.Application.Common.Interfaces;
using BehaAtel.Application.Common.Models;
using BehaAtel.Application.Features.Rooms.Common;
using BehaAtel.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BehaAtel.Application.Features.Rooms.GetRooms;

public sealed class GetRoomsHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetRoomsQuery, PagedList<RoomDto>>
{
    public async Task<PagedList<RoomDto>> Handle(GetRoomsQuery request, CancellationToken cancellationToken)
    {
        var rooms = dbContext.Rooms.AsNoTracking().Where(room => !room.IsDeleted);

        if (request.BuildingId.HasValue)
        {
            rooms = rooms.Where(room => room.BuildingId == request.BuildingId.Value);
        }

        if (request.Floor.HasValue)
        {
            rooms = rooms.Where(room => room.Floor == request.Floor.Value);
        }

        if (request.Type.HasValue)
        {
            rooms = rooms.Where(room => room.Type == request.Type.Value);
        }

        if (request.OnlyWithFreeBeds == true)
        {
            rooms = rooms.Where(room =>
                dbContext.Occupancies.Count(occupancy =>
                    occupancy.RoomId == room.Id && occupancy.To == null) < room.Capacity);
        }

        var query =
            from room in rooms
            join building in dbContext.Buildings.AsNoTracking().Where(building => !building.IsDeleted)
                on room.BuildingId equals building.Id
            select new
            {
                Room = room,
                BuildingName = building.Name,
                OccupiedBeds = dbContext.Occupancies.Count(occupancy =>
                    occupancy.RoomId == room.Id && occupancy.To == null)
            };

        var descending = string.Equals(request.Pagination.SortDir, "desc", StringComparison.OrdinalIgnoreCase);
        query = (request.Pagination.SortBy?.Trim().ToLower(), descending) switch
        {
            ("number", false) => query.OrderBy(item => item.Room.Number),
            ("number", true) => query.OrderByDescending(item => item.Room.Number),
            ("floor", false) => query.OrderBy(item => item.Room.Floor),
            ("floor", true) => query.OrderByDescending(item => item.Room.Floor),
            ("capacity", false) => query.OrderBy(item => item.Room.Capacity),
            ("capacity", true) => query.OrderByDescending(item => item.Room.Capacity),
            ("occupiedbeds", false) => query.OrderBy(item => item.OccupiedBeds),
            ("occupiedbeds", true) => query.OrderByDescending(item => item.OccupiedBeds),
            ("priceperbed", false) => query.OrderBy(item => item.Room.PricePerBed),
            ("priceperbed", true) => query.OrderByDescending(item => item.Room.PricePerBed),
            (_, false) => query.OrderBy(item => item.BuildingName).ThenBy(item => item.Room.Number),
            _ => query.OrderByDescending(item => item.BuildingName).ThenByDescending(item => item.Room.Number)
        };

        return await PagedList<RoomDto>.CreateAsync(
            query.Select(item => new RoomDto(
                item.Room.Id,
                item.Room.BuildingId,
                item.BuildingName,
                item.Room.Number,
                item.Room.Floor,
                item.Room.Capacity,
                item.OccupiedBeds,
                item.Room.Capacity - item.OccupiedBeds,
                item.Room.PricePerBed,
                item.Room.Type)),
            request.Pagination.Page,
            request.Pagination.PageSize,
            cancellationToken);
    }
}
