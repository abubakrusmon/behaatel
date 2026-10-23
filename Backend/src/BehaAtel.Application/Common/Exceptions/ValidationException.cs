namespace BehaAtel.Application.Common.Exceptions;

public sealed class ValidationException : Exception
{
    public ValidationException(Dictionary<string, string[]> errors)
        : base("Обнаружены ошибки проверки данных.")
    {
        Errors = errors;
    }

    public Dictionary<string, string[]> Errors { get; }
}
