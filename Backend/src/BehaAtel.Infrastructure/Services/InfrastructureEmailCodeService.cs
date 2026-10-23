using System.Security.Cryptography;
using System.Text;
using BehaAtel.Application.Common.Interfaces;
using BehaAtel.Domain.Entities;
using Microsoft.Extensions.Configuration;

namespace BehaAtel.Infrastructure.Services;

public sealed class InfrastructureEmailCodeService : IEmailCodeService
{
    private readonly byte[] _key;

    public InfrastructureEmailCodeService(IConfiguration configuration)
    {
        var secret = configuration["Auth:EmailCodePepper"] ?? configuration["Jwt:Key"];
        if (string.IsNullOrWhiteSpace(secret))
            throw new InvalidOperationException("Не задан ключ для хэширования email-кодов.");
        _key = Encoding.UTF8.GetBytes(secret);
    }

    public string GenerateCode() => RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");

    public string HashCode(string code)
    {
        using var hmac = new HMACSHA256(_key);
        return Convert.ToHexString(hmac.ComputeHash(Encoding.UTF8.GetBytes(code)));
    }

    public bool VerifyCode(EmailCode emailCode, string code, DateTime utcNow) =>
        emailCode.TryVerify(HashCode(code), utcNow);
}
