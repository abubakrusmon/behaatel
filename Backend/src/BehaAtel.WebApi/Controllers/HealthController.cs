using BehaAtel.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BehaAtel.WebApi.Controllers;

[ApiController]
[Route("health")]
public sealed class HealthController(ApplicationDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        if (!await dbContext.Database.CanConnectAsync(cancellationToken))
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new ProblemDetails
            {
                Type = "about:blank",
                Title = "База данных недоступна.",
                Status = StatusCodes.Status503ServiceUnavailable,
                Detail = "Не удалось подключиться к базе данных.",
                Extensions = { ["traceId"] = HttpContext.TraceIdentifier }
            });
        }

        return Ok(new { status = "healthy" });
    }
}
