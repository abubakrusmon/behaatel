using BehaAtel.Domain.Common;
using BehaAtel.Domain.Enums;
using BehaAtel.Domain.Exceptions;

namespace BehaAtel.Domain.Entities;

public class User : BaseAuditableEntity, ISoftDelete
{
    private readonly List<AdminPermission> _permissions = [];
    private readonly List<RefreshToken> _refreshTokens = [];

    private User()
    {
        Email = string.Empty;
        PasswordHash = string.Empty;
        FullName = string.Empty;
    }

    private User(string email, string passwordHash, string fullName, Role role, string? phone, bool mustChangePassword)
    {
        Email = Required(email, nameof(email));
        PasswordHash = Required(passwordHash, nameof(passwordHash));
        FullName = Required(fullName, nameof(fullName));
        Role = role;
        Phone = NormalizeOptional(phone);
        IsActive = true;
        MustChangePassword = mustChangePassword;
    }

    public string Email { get; private set; }
    public string PasswordHash { get; private set; }
    public string FullName { get; private set; }
    public string? Phone { get; private set; }
    public Role Role { get; private set; }
    public bool IsActive { get; private set; }
    public bool EmailConfirmed { get; private set; } = true;
    public ApprovalStatus ApprovalStatus { get; private set; } = ApprovalStatus.Approved;
    public DateTime? RegistrationRequestedAt { get; private set; }
    public string? RegistrationPassportNumber { get; private set; }
    public DateOnly? RegistrationBirthDate { get; private set; }
    public string? RegistrationOrganization { get; private set; }
    public bool MustChangePassword { get; private set; }
    public DateTime? LastLoginAt { get; private set; }
    public int FailedLoginAttempts { get; private set; }
    public DateTime? LockoutEnd { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAt { get; private set; }
    public IReadOnlyCollection<AdminPermission> Permissions => _permissions.AsReadOnly();
    public IReadOnlyCollection<RefreshToken> RefreshTokens => _refreshTokens.AsReadOnly();

    public static User Create(string email, string passwordHash, string fullName, Role role = Role.User, string? phone = null, bool mustChangePassword = false)
    {
        var normalizedEmail = Required(email, nameof(email));
        if (!normalizedEmail.Contains('@', StringComparison.Ordinal))
        {
            throw new DomainException("Email должен содержать символ @.");
        }

        if (!Enum.IsDefined(role))
        {
            throw new DomainException("Указана неизвестная роль.");
        }

        return new User(normalizedEmail, passwordHash, fullName, role, phone, mustChangePassword);
    }

    public static User CreateRegistration(
        string email,
        string passwordHash,
        string fullName,
        string? phone,
        string passportNumber,
        DateOnly birthDate,
        string? organization,
        DateTime requestedAt)
    {
        if (requestedAt.Kind != DateTimeKind.Utc) throw new DomainException("Дата заявки должна быть в UTC.");
        if (string.IsNullOrWhiteSpace(passportNumber)) throw new DomainException("Номер паспорта обязателен.");
        if (birthDate >= DateOnly.FromDateTime(requestedAt)) throw new DomainException("Дата рождения должна быть в прошлом.");
        var user = Create(email, passwordHash, fullName, Role.User, phone);
        user.IsActive = false;
        user.EmailConfirmed = false;
        user.ApprovalStatus = ApprovalStatus.Pending;
        user.RegistrationRequestedAt = requestedAt;
        user.RegistrationPassportNumber = passportNumber.Trim();
        user.RegistrationBirthDate = birthDate;
        user.RegistrationOrganization = NormalizeOptional(organization);
        return user;
    }

    public void ReopenRejectedRegistration(
        string passwordHash,
        string fullName,
        string? phone,
        string passportNumber,
        DateOnly birthDate,
        string? organization,
        DateTime requestedAt)
    {
        if (Role != Role.User || ApprovalStatus != ApprovalStatus.Rejected)
            throw new DomainException("Можно повторно подать только отклонённую заявку жильца.");
        if (requestedAt.Kind != DateTimeKind.Utc)
            throw new DomainException("Дата заявки должна быть в UTC.");
        if (string.IsNullOrWhiteSpace(passportNumber))
            throw new DomainException("Номер паспорта обязателен.");
        if (birthDate >= DateOnly.FromDateTime(requestedAt))
            throw new DomainException("Дата рождения должна быть в прошлом.");

        PasswordHash = Required(passwordHash, nameof(passwordHash));
        FullName = Required(fullName, nameof(fullName));
        Phone = NormalizeOptional(phone);
        RegistrationPassportNumber = passportNumber.Trim();
        RegistrationBirthDate = birthDate;
        RegistrationOrganization = NormalizeOptional(organization);
        RegistrationRequestedAt = requestedAt;
        ApprovalStatus = ApprovalStatus.Pending;
        EmailConfirmed = false;
        IsActive = false;
        FailedLoginAttempts = 0;
        LockoutEnd = null;
    }

    public void ConfirmEmail() => EmailConfirmed = true;

    public void ApproveRegistration()
    {
        if (Role != Role.User || ApprovalStatus != ApprovalStatus.Pending || !EmailConfirmed)
            throw new DomainException("Нельзя одобрить эту заявку.");
        ApprovalStatus = ApprovalStatus.Approved;
        IsActive = true;
    }

    public void RejectRegistration()
    {
        if (Role != Role.User || ApprovalStatus != ApprovalStatus.Pending)
            throw new DomainException("Нельзя отклонить эту заявку.");
        ApprovalStatus = ApprovalStatus.Rejected;
        IsActive = false;
    }

    public (string PassportNumber, DateOnly BirthDate, string? Organization) TakeRegistrationProfile()
    {
        if (ApprovalStatus != ApprovalStatus.Approved ||
            RegistrationPassportNumber is null ||
            RegistrationBirthDate is null)
            throw new DomainException("Профиль регистрации отсутствует.");
        var profile = (RegistrationPassportNumber, RegistrationBirthDate.Value, RegistrationOrganization);
        RegistrationPassportNumber = null;
        RegistrationBirthDate = null;
        RegistrationOrganization = null;
        return profile;
    }

    public void Block()
    {
        EnsureNotSuperAdmin();
        IsActive = false;
    }

    public void Unblock()
    {
        IsActive = true;
        FailedLoginAttempts = 0;
        LockoutEnd = null;
    }

    public void Delete()
    {
        EnsureNotSuperAdmin();
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
        IsActive = false;
    }

    public void SetPassword(string hash, bool mustChange)
    {
        PasswordHash = Required(hash, nameof(hash));
        MustChangePassword = mustChange;
        FailedLoginAttempts = 0;
        LockoutEnd = null;
    }

    public void UpdateProfile(string fullName, string? phone)
    {
        FullName = Required(fullName, nameof(fullName));
        Phone = NormalizeOptional(phone);
    }

    public void GrantPermission(Permission permission)
    {
        if (!Enum.IsDefined(permission))
        {
            throw new DomainException("Указано неизвестное право.");
        }

        if (Role == Role.SuperAdmin)
        {
            return;
        }

        if (_permissions.All(x => x.Permission != permission))
        {
            _permissions.Add(AdminPermission.Create(Id, permission));
        }
    }

    public void RevokePermission(Permission permission) =>
        _permissions.RemoveAll(x => x.Permission == ValidatePermission(permission));

    public void SetPermissions(IEnumerable<Permission> permissions)
    {
        if (permissions is null)
        {
            throw new DomainException("Список прав не может быть null.");
        }

        var requestedPermissions = permissions
            .Distinct()
            .Select(ValidatePermission)
            .ToArray();

        _permissions.Clear();

        if (Role == Role.SuperAdmin)
        {
            return;
        }

        foreach (var permission in requestedPermissions)
        {
            _permissions.Add(AdminPermission.Create(Id, permission));
        }
    }

    public bool HasPermission(Permission permission)
    {
        ValidatePermission(permission);
        return Role == Role.SuperAdmin || _permissions.Any(x => x.Permission == permission);
    }

    public void RegisterLogin()
    {
        LastLoginAt = DateTime.UtcNow;
        FailedLoginAttempts = 0;
        LockoutEnd = null;
    }

    public void RegisterFailedLogin()
    {
        FailedLoginAttempts++;
        if (FailedLoginAttempts >= 5)
        {
            LockoutEnd = DateTime.UtcNow.AddMinutes(15);
        }
    }

    internal void AddRefreshToken(RefreshToken token)
    {
        ArgumentNullException.ThrowIfNull(token);
        if (token.UserId != Id)
        {
            throw new DomainException("Токен не принадлежит пользователю.");
        }

        _refreshTokens.Add(token);
    }

    private void EnsureNotSuperAdmin()
    {
        if (Role == Role.SuperAdmin)
        {
            throw new DomainException("SuperAdmin нельзя заблокировать или удалить.");
        }
    }

    private static Permission ValidatePermission(Permission permission)
    {
        if (!Enum.IsDefined(permission))
        {
            throw new DomainException("Указано неизвестное право.");
        }

        return permission;
    }

    private static string Required(string value, string field)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException($"{field} не может быть пустым.");
        }

        return value.Trim();
    }

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
