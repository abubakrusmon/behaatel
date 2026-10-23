using BehaAtel.Domain.Common;
using BehaAtel.Domain.Exceptions;

namespace BehaAtel.Domain.Entities;

public class Building : BaseAuditableEntity, ISoftDelete
{
    private readonly List<Room> _rooms = [];

    private Building()
    {
        Name = string.Empty;
        Address = string.Empty;
    }

    private Building(string name, string address, int floorsCount)
    {
        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(address))
        {
            throw new DomainException("Название и адрес здания обязательны.");
        }

        if (floorsCount <= 0)
        {
            throw new DomainException("Количество этажей должно быть больше нуля.");
        }

        Name = name.Trim();
        Address = address.Trim();
        FloorsCount = floorsCount;
    }

    public string Name { get; private set; }
    public string Address { get; private set; }
    public int FloorsCount { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAt { get; private set; }
    public IReadOnlyCollection<Room> Rooms => _rooms.AsReadOnly();

    public static Building Create(string name, string address, int floorsCount) =>
        new(name, address, floorsCount);

    public void Update(string name, string address, int floorsCount)
    {
        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(address))
        {
            throw new DomainException("Название и адрес здания обязательны.");
        }

        if (floorsCount <= 0)
        {
            throw new DomainException("Количество этажей должно быть больше нуля.");
        }

        Name = name.Trim();
        Address = address.Trim();
        FloorsCount = floorsCount;
    }

    public void MarkDeleted()
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
    }

    public void AddRoom(Room room)
    {
        ArgumentNullException.ThrowIfNull(room);
        if (room.BuildingId != Id)
        {
            throw new DomainException("Комната принадлежит другому зданию.");
        }

        _rooms.Add(room);
    }
}
