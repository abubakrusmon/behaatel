using BehaAtel.Application.Features.Buildings.Common;
using BehaAtel.Application.Features.Rooms.Common;
using BehaAtel.Domain.Entities;
using BehaAtel.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace BehaAtel.WebApi.IntegrationTests;

public sealed class PersistenceModelTests
{
    [Fact]
    public void Model_HasSoftDeleteFilters_WithoutRequiredNavigationWarnings()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql("Host=localhost;Database=behaatel_test;Username=postgres;Password=postgres")
            .ConfigureWarnings(warnings =>
                warnings.Throw(CoreEventId.PossibleIncorrectRequiredNavigationWithQueryFilterInteractionWarning))
            .Options;

        using var context = new ApplicationDbContext(options);
        var model = context.Model;

        Assert.NotEmpty(model.FindEntityType(typeof(User))!.GetDeclaredQueryFilters());
        Assert.NotEmpty(model.FindEntityType(typeof(Resident))!.GetDeclaredQueryFilters());
        Assert.NotEmpty(model.FindEntityType(typeof(Building))!.GetDeclaredQueryFilters());
        Assert.NotEmpty(model.FindEntityType(typeof(Room))!.GetDeclaredQueryFilters());
        Assert.NotEmpty(model.FindEntityType(typeof(Announcement))!.GetDeclaredQueryFilters());
    }

    [Fact]
    public void RoomAndBuildingReadQueries_AreTranslatedToSqlWithoutConnectingToDatabase()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql("Host=localhost;Database=behaatel_test;Username=postgres;******")
            .Options;

        using var context = new ApplicationDbContext(options);
        context.Database.SetConnectionString("Host=localhost;Database=behaatel_test;Username=postgres;Password=test");
        var currentOccupancies = context.Occupancies
            .AsNoTracking()
            .Where(occupancy => occupancy.To == null);
        var rooms = context.Rooms
            .AsNoTracking()
            .Where(room => !room.IsDeleted)
            .Where(room => currentOccupancies.Count(occupancy => occupancy.RoomId == room.Id) < room.Capacity);
        var roomQuery =
            from room in rooms
            join building in context.Buildings.AsNoTracking().Where(building => !building.IsDeleted)
                on room.BuildingId equals building.Id
            select new
            {
                Room = room,
                BuildingName = building.Name,
                OccupiedBeds = currentOccupancies.Count(occupancy => occupancy.RoomId == room.Id)
            };
        var roomSql = roomQuery
            .OrderBy(item => item.OccupiedBeds)
            .Select(item => new RoomDto(
                item.Room.Id,
                item.Room.BuildingId,
                item.BuildingName,
                item.Room.Number,
                item.Room.Floor,
                item.Room.Capacity,
                item.OccupiedBeds,
                item.Room.Capacity - item.OccupiedBeds,
                item.Room.PricePerBed,
                item.Room.Type))
            .ToQueryString();

        var buildingsSql = context.Buildings
            .AsNoTracking()
            .Where(building => !building.IsDeleted)
            .Select(building => new BuildingDto(
                building.Id,
                building.Name,
                building.Address,
                building.FloorsCount,
                context.Rooms.Count(room => room.BuildingId == building.Id && !room.IsDeleted),
                context.Rooms
                    .Where(room => room.BuildingId == building.Id && !room.IsDeleted)
                    .Sum(room => (int?)room.Capacity) ?? 0,
                (from room in context.Rooms
                 where room.BuildingId == building.Id && !room.IsDeleted
                 from occupancy in context.Occupancies
                 where occupancy.RoomId == room.Id && occupancy.To == null
                 select occupancy.Id).Count()))
            .ToQueryString();

        Assert.Contains("COUNT", roomSql.ToUpperInvariant());
        Assert.Contains("COUNT", buildingsSql.ToUpperInvariant());
        Assert.Contains("SUM", buildingsSql.ToUpperInvariant());
    }
}
