using System.Net;
using System.Net.Mail;
using BehaAtel.Application.Common.Interfaces;
using BehaAtel.Application.Features.Auth.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BehaAtel.Infrastructure.Services;

public sealed class SmtpEmailSender(
    IConfiguration configuration,
    IHostEnvironment environment,
    ILogger<SmtpEmailSender> logger) : IEmailSender
{
    public async Task SendAsync(string to, string subject, string htmlBody, CancellationToken cancellationToken)
    {
        var section = configuration.GetSection("Email");
        var password = section["Password"];
        if (string.IsNullOrWhiteSpace(password) && environment.IsDevelopment())
        {
            logger.LogWarning(
                "Email not sent because SMTP password is not configured in Development. Recipient {Recipient}; subject {Subject}.",
                MaskEmail(to), subject);
            return;
        }

        try
        {
            var host = section["Host"] ?? "smtp.gmail.com";
            var port = section.GetValue("Port", 587);
            var fromAddress = section["FromAddress"];
            if (string.IsNullOrWhiteSpace(fromAddress))
                throw new InvalidOperationException("Не задан Email:FromAddress.");
            using var message = new MailMessage
            {
                From = new MailAddress(fromAddress, section["FromName"] ?? "BehaAtel"),
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true
            };
            message.To.Add(to);
            using var client = new SmtpClient(host, port)
            {
                EnableSsl = section.GetValue("EnableSsl", true),
                Credentials = new NetworkCredential(section["User"], password)
            };
            await client.SendMailAsync(message, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            logger.LogError(
                "Email send failed. Recipient {Recipient}; subject {Subject}; error type {ErrorType}.",
                EmailCodePolicy.MaskEmail(to), subject, exception.GetType().Name);
            throw;
        }
    }

    private static string MaskEmail(string email) => EmailCodePolicy.MaskEmail(email);
}
