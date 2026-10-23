namespace BehaAtel.Application.Common.Exceptions;

public sealed class ForbiddenException : Exception
{
    public ForbiddenException(string message = "Недостаточно прав для выполнения операции.")
        : base(message)
    {
    }
}
