using BehaAtel.Application.Common.Interfaces;
using BehaAtel.Application.Common.Models;
using BehaAtel.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BehaAtel.WebApi.Controllers;

[ApiController]
[Authorize(Policy = "SuperAdminOnly")]
[Route("api/v1/admin/telegram")]
public sealed class AdminTelegramController(ITelegramNotifier telegramNotifier) : ControllerBase
{
    [HttpPost("test")]
    public IActionResult Test()
    {
        var status = telegramNotifier.Enqueue(new TelegramNotificationJob(
            "<b>BehaAtel</b>\nПроверка соединения с Telegram."));
        return Ok(new { telegramStatus = status });
    }
}
