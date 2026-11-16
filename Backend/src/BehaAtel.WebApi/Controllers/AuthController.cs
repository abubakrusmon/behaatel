using BehaAtel.Application.Features.Auth.ChangePassword;
using BehaAtel.Application.Features.Auth.UpdateAvatar;
using BehaAtel.Application.Features.Auth.GetCurrentUser;
using BehaAtel.Application.Features.Auth.Login;
using BehaAtel.Application.Features.Auth.Register;
using BehaAtel.Application.Features.Auth.VerifyEmail;
using BehaAtel.Application.Features.Auth.ResendVerification;
using BehaAtel.Application.Features.Auth.ForgotPassword;
using BehaAtel.Application.Features.Auth.ResetPassword;
using BehaAtel.Application.Features.Auth.VerifyLoginCode;
using BehaAtel.Application.Features.Auth.ResendLoginCode;
using BehaAtel.Application.Features.Auth.Logout;
using BehaAtel.Application.Features.Auth.Refresh;
using BehaAtel.Application.Common.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace BehaAtel.WebApi.Controllers;

[ApiController]
[Route("api/v1/auth")]
public sealed class AuthController(IMediator mediator, ITelegramAccountLinkService telegramLinkService) : ControllerBase
{
    [AllowAnonymous]
    [EnableRateLimiting("auth-public")]
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginCommand command, CancellationToken cancellationToken) =>
        Ok(await mediator.Send(command, cancellationToken));

    [AllowAnonymous]
    [EnableRateLimiting("auth-public")]
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(RefreshTokenCommand command, CancellationToken cancellationToken) =>
        Ok(await mediator.Send(command, cancellationToken));

    [AllowAnonymous]
    [EnableRateLimiting("auth-public")]
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterCommand command, CancellationToken cancellationToken) =>
        Ok(await mediator.Send(command, cancellationToken));

    [AllowAnonymous]
    [EnableRateLimiting("auth-public")]
    [HttpPost("verify-email")]
    public async Task<IActionResult> VerifyEmail(VerifyEmailCommand command, CancellationToken cancellationToken) =>
        Ok(await mediator.Send(command, cancellationToken));

    [AllowAnonymous]
    [EnableRateLimiting("auth-public")]
    [HttpPost("resend-verification")]
    public async Task<IActionResult> ResendVerification(ResendVerificationCommand command, CancellationToken cancellationToken) =>
        Ok(await mediator.Send(command, cancellationToken));

    [AllowAnonymous]
    [EnableRateLimiting("auth-public")]
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordCommand command, CancellationToken cancellationToken) =>
        Ok(await mediator.Send(command, cancellationToken));

    [AllowAnonymous]
    [EnableRateLimiting("auth-public")]
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(ResetPasswordCommand command, CancellationToken cancellationToken) =>
        Ok(await mediator.Send(command, cancellationToken));

    [AllowAnonymous]
    [EnableRateLimiting("auth-public")]
    [HttpPost("login/verify-code")]
    public async Task<IActionResult> VerifyLoginCode(VerifyLoginCodeCommand command, CancellationToken cancellationToken) =>
        Ok(await mediator.Send(command, cancellationToken));

    [AllowAnonymous]
    [EnableRateLimiting("auth-public")]
    [HttpPost("telegram-link/status")]
    public async Task<IActionResult> TelegramLinkStatus(
        TelegramLinkStatusRequest request,
        CancellationToken cancellationToken) =>
        Ok(await telegramLinkService.PollAsync(request.AttemptId, request.PollToken, cancellationToken));

    [AllowAnonymous]
    [EnableRateLimiting("auth-public")]
    [HttpPost("login/resend-code")]
    public async Task<IActionResult> ResendLoginCode(ResendLoginCodeCommand command, CancellationToken cancellationToken) =>
        Ok(await mediator.Send(command, cancellationToken));

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout(LogoutCommand command, CancellationToken cancellationToken)
    {
        await mediator.Send(command, cancellationToken);
        return NoContent();
    }

    public sealed record TelegramLinkStatusRequest(Guid AttemptId, string PollToken);

    [Authorize]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword(ChangePasswordCommand command, CancellationToken cancellationToken)
    {
        await mediator.Send(command, cancellationToken);
        return NoContent();
    }

    [Authorize]
    [HttpPut("avatar")]
    public async Task<IActionResult> UpdateAvatar(UpdateAvatarCommand command, CancellationToken cancellationToken) =>
        Ok(await mediator.Send(command, cancellationToken));

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser(CancellationToken cancellationToken) =>
        Ok(await mediator.Send(new GetCurrentUserQuery(), cancellationToken));
}
