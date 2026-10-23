using BehaAtel.Domain.Common;
using BehaAtel.Domain.Exceptions;

namespace BehaAtel.Domain.Entities;

public class Occupancy : BaseEntity
{
    private Occupancy()
    {
    }

    private Occupancy(Guid residentId, Guid roomId, DateOnly from)
    {
        if (residentId == Guid.Empty || roomId == Guid.Empty)
        {
            throw new DomainException("ResidentId и RoomId обязательны.");
        }

        ResidentId = residentId;
        RoomId = roomId;
        From = from;
    }

    public Guid ResidentId { get; private set; }
    public Guid RoomId { get; private set; }
    public DateOnly From { get; private set; }
    public DateOnly? To { get; private set; }

    public static Occupancy Create(Guid residentId, Guid roomId, DateOnly from) =>
        new(residentId, roomId, from);

    public void Close(DateOnly to)
    {
        if (To.HasValue)
        {
            throw new DomainException("Заселение уже закрыто.");
        }

        if (to < From)
        {
            throw new DomainException("Дата выселения не может быть раньше даты заселения.");
        }

        To = to;
    }
}
