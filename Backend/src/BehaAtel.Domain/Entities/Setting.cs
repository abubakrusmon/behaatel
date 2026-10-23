using BehaAtel.Domain.Exceptions;

namespace BehaAtel.Domain.Entities;

public class Setting
{
    private Setting()
    {
        Key = string.Empty;
        Value = string.Empty;
    }

    private Setting(string key, string value, DateTime updatedAt)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new DomainException("Ключ настройки не может быть пустым.");
        }

        if (updatedAt.Kind != DateTimeKind.Utc)
        {
            throw new DomainException("UpdatedAt должен быть в UTC.");
        }

        Key = key.Trim();
        Value = value ?? throw new DomainException("Значение настройки обязательно.");
        UpdatedAt = updatedAt;
    }

    public string Key { get; private set; }
    public string Value { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public static Setting Create(string key, string value, DateTime? updatedAt = null) =>
        new(key, value, updatedAt ?? DateTime.UtcNow);

    public void UpdateValue(string value)
    {
        UpdateValue(value, DateTime.UtcNow);
    }

    public void UpdateValue(string value, DateTime updatedAt)
    {
        if (value is null)
        {
            throw new DomainException("Значение настройки обязательно.");
        }

        if (updatedAt.Kind != DateTimeKind.Utc)
        {
            throw new DomainException("UpdatedAt должен быть в UTC.");
        }

        Value = value;
        UpdatedAt = updatedAt;
    }
}
