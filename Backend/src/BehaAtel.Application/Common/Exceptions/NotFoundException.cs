namespace BehaAtel.Application.Common.Exceptions;

public sealed class NotFoundException : Exception
{
    public NotFoundException(string entityName, object key)
        : base($"Сущность '{entityName}' с ключом '{key}' не найдена.")
    {
    }
}
