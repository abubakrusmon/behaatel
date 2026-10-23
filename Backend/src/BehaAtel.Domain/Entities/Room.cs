using BehaAtel.Domain.Common;
using BehaAtel.Domain.Enums;
using BehaAtel.Domain.Exceptions;

namespace BehaAtel.Domain.Entities;

public class Room : BaseAuditableEntity, ISoftDelete
{
    private readonly List<Occupancy> _occupancies = [];

    private Room()
    {
        Number = string.Empty;
    }

    private Room(Guid buildingId, string number, int floor, int capacity, decimal pricePerBed, RoomType type)
    {
        if (buildingId == Guid.Empty)
        {
            throw new DomainException("BuildingId не может быть пустым.");
        }

        if (string.IsNullOrWhiteSpace(number))
        {
            throw new DomainException("Номер комнаты не может быть пустым.");
        }

        if (floor <= 0 || capacity <= 0 || pricePerBed < 0)
        {
            throw new DomainException("Этаж и вместимость должны быть больше нуля, цена не может быть отрицательной.");
        }

        if (!Enum.IsDefined(type))
        {
            throw new DomainException("Указан неизвестный тип комнаты.");
        }

        BuildingId = buildingId;
        Number = number.Trim();
        Floor = floor;
        Capacity = capacity;
        PricePerBed = pricePerBed;
        Type = type;
    }

    public Guid BuildingId { get; private set; }
    public string Number { get; private set; }
    public int Floor { get; private set; }
    public int Capacity { get; private set; }
    public decimal PricePerBed { get; private set; }
    public RoomType Type { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAt { get; private set; }
    public IReadOnlyCollection<Occupancy> Occupancies => _occupancies.AsReadOnly();
    public int CurrentOccupantsCount => _occupancies.Count(x => x.To is null);
    public bool IsFull => CurrentOccupantsCount >= Capacity;
    public int FreeBeds => Math.Max(0, Capacity - CurrentOccupantsCount);

    public static Room Create(Guid buildingId, string number, int floor, int capacity, decimal pricePerBed, RoomType type) =>
        new(buildingId, number, floor, capacity, pricePerBed, type);

    public void Update(string number, int floor, int capacity, decimal pricePerBed, RoomType type)
    {
        if (string.IsNullOrWhiteSpace(number))
        {
            throw new DomainException("Номер комнаты не может быть пустым.");
        }

        if (floor <= 0 || capacity <= 0 || pricePerBed < 0)
        {
            throw new DomainException("Этаж и вместимость должны быть больше нуля, цена не может быть отрицательной.");
        }

        if (!Enum.IsDefined(type))
        {
            throw new DomainException("Указан неизвестный тип комнаты.");
        }

        if (capacity < CurrentOccupantsCount)
        {
            throw new DomainException("Нельзя уменьшить вместимость ниже числа проживающих.");
        }

        Number = number.Trim();
        Floor = floor;
        Capacity = capacity;
        PricePerBed = pricePerBed;
        Type = type;
    }

    public void MarkDeleted()
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
    }

    internal void AddOccupancy(Occupancy occupancy)
    {
        ArgumentNullException.ThrowIfNull(occupancy);
        if (occupancy.RoomId != Id)
        {
            throw new DomainException("Заселение относится к другой комнате.");
        }

        if (IsFull)
        {
            throw new DomainException("В комнате нет свободных мест.");
        }

        _occupancies.Add(occupancy);
    }
}
