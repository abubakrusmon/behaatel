using BehaAtel.Domain.Common;
using BehaAtel.Domain.Enums;
using BehaAtel.Domain.Exceptions;

namespace BehaAtel.Domain.Entities;

public class Resident : BaseAuditableEntity, ISoftDelete
{
    private readonly List<Occupancy> _occupancies = [];
    private readonly List<Charge> _charges = [];
    private readonly List<Payment> _payments = [];

    private Resident()
    {
        PassportNumber = string.Empty;
    }

    private Resident(Guid userId, string passportNumber, DateOnly birthDate, string? organization)
    {
        if (userId == Guid.Empty)
        {
            throw new DomainException("UserId не может быть пустым.");
        }

        if (string.IsNullOrWhiteSpace(passportNumber))
        {
            throw new DomainException("Номер паспорта не может быть пустым.");
        }

        if (birthDate >= DateOnly.FromDateTime(DateTime.UtcNow))
        {
            throw new DomainException("Дата рождения должна быть в прошлом.");
        }

        UserId = userId;
        PassportNumber = passportNumber.Trim();
        BirthDate = birthDate;
        Organization = string.IsNullOrWhiteSpace(organization) ? null : organization.Trim();
        Status = ResidentStatus.CheckedOut;
    }

    public Guid UserId { get; private set; }
    public string PassportNumber { get; private set; }
    public DateOnly BirthDate { get; private set; }
    public string? Organization { get; private set; }
    public DateOnly? CheckInDate { get; private set; }
    public DateOnly? CheckOutDate { get; private set; }
    public ResidentStatus Status { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAt { get; private set; }
    public IReadOnlyCollection<Occupancy> Occupancies => _occupancies.AsReadOnly();
    public IReadOnlyCollection<Charge> Charges => _charges.AsReadOnly();
    public IReadOnlyCollection<Payment> Payments => _payments.AsReadOnly();
    public decimal Balance =>
        _payments.Where(x => x.Status == PaymentStatus.Completed).Sum(x => x.Amount)
        - _charges.Where(x => x.Status != ChargeStatus.Cancelled).Sum(x => x.Amount);

    public static Resident Create(Guid userId, string passportNumber, DateOnly birthDate, string? organization = null) =>
        new(userId, passportNumber, birthDate, organization);

    public void Update(string passportNumber, DateOnly birthDate, string? organization)
    {
        if (string.IsNullOrWhiteSpace(passportNumber))
        {
            throw new DomainException("Номер паспорта не может быть пустым.");
        }

        if (birthDate >= DateOnly.FromDateTime(DateTime.UtcNow))
        {
            throw new DomainException("Дата рождения должна быть в прошлом.");
        }

        PassportNumber = passportNumber.Trim();
        BirthDate = birthDate;
        Organization = string.IsNullOrWhiteSpace(organization) ? null : organization.Trim();
    }

    public void MarkDeleted()
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
    }

    public void CheckIn(Room room, DateOnly date)
    {
        if (room is null)
        {
            throw new DomainException("Комната обязательна для заселения.");
        }

        if (Status == ResidentStatus.Active || _occupancies.Any(x => x.To is null))
        {
            throw new DomainException("Жилец уже заселён.");
        }

        if (room.IsFull)
        {
            throw new DomainException("В комнате нет свободных мест.");
        }

        var occupancy = Occupancy.Create(Id, room.Id, date);
        _occupancies.Add(occupancy);
        room.AddOccupancy(occupancy);
        CheckInDate = date;
        CheckOutDate = null;
        Status = ResidentStatus.Active;
    }

    public void CheckOut(DateOnly date)
    {
        var occupancy = _occupancies.SingleOrDefault(x => x.To is null);
        if (Status != ResidentStatus.Active || occupancy is null)
        {
            throw new DomainException("Жилец не заселён.");
        }

        occupancy.Close(date);
        CheckOutDate = date;
        Status = ResidentStatus.CheckedOut;
    }

    internal void AddCharge(Charge charge)
    {
        ArgumentNullException.ThrowIfNull(charge);
        if (charge.ResidentId != Id)
        {
            throw new DomainException("Начисление принадлежит другому жильцу.");
        }

        _charges.Add(charge);
    }

    internal void AddPayment(Payment payment)
    {
        ArgumentNullException.ThrowIfNull(payment);
        if (payment.ResidentId != Id)
        {
            throw new DomainException("Платёж принадлежит другому жильцу.");
        }

        _payments.Add(payment);
    }
}
