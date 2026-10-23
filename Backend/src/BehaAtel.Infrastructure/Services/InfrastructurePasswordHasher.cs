using BehaAtel.Application.Common.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace BehaAtel.Infrastructure.Services;

public sealed class InfrastructurePasswordHasher : IPasswordHasher
{
    private readonly PasswordHasher<object> _passwordHasher = new();

    public string Hash(string password)
    {
        ArgumentNullException.ThrowIfNull(password);
        return _passwordHasher.HashPassword(new object(), password);
    }

    public bool Verify(string hash, string password)
    {
        ArgumentNullException.ThrowIfNull(hash);
        ArgumentNullException.ThrowIfNull(password);
        var result = _passwordHasher.VerifyHashedPassword(new object(), hash, password);
        return result is PasswordVerificationResult.Success or PasswordVerificationResult.SuccessRehashNeeded;
    }
}
