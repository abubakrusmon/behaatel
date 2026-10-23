using BehaAtel.Domain.Entities;

namespace BehaAtel.Application.Common.Interfaces;

public interface IEmailCodeService
{
    string GenerateCode();
    string HashCode(string code);
    bool VerifyCode(EmailCode emailCode, string code, DateTime utcNow);
}
