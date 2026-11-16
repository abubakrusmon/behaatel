using System.Security.Claims;
using BehaAtel.Application.Common.Interfaces;
using BehaAtel.WebApi.Middleware;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace BehaAtel.WebApi.Middleware;

public sealed class TelegramLinkRequiredMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(
        HttpContext context,
        IApplicationDbContext db,
        ITelegramAccountLinkService telegramLinkService)
    {
        if (telegramLinkService.IsConfigured &&
            context.User.Identity?.IsAuthenticated == true &&
            context.GetEndpoint()?.Metadata.GetMetadata<IAllowAnonymous>() is null &&
            Guid.TryParse(context.User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
        {
            var requiresLink = await db.Users
                .Where(user => user.Id == userId)
                .Select(user => user.RequiresTelegramLink)
                .SingleOrDefaultAsync(context.RequestAborted);
            if (requiresLink)
            {
                await ProblemDetailsWriter.WriteAsync(
                    context,
                    StatusCodes.Status403Forbidden,
                    "Требуется привязка Telegram.",
                    "Завершите привязку Telegram, чтобы продолжить.");
                return;
            }
        }

        await next(context);
    }
}
