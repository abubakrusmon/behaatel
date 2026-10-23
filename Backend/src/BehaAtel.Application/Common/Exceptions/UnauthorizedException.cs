namespace BehaAtel.Application.Common.Exceptions;

public sealed class UnauthorizedException : Exception
{
    public UnauthorizedException(string message = "Неверный email или пароль")
        : base(message)
    {
    }
}
