using FiscalGuard.Application;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FiscalGuard.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("register")]
    [AllowAnonymous]
    public Task<AuthResponse> Register(RegisterOrganizationRequest request, CancellationToken cancellationToken) =>
        authService.RegisterAsync(request, cancellationToken);

    [HttpPost("login")]
    [AllowAnonymous]
    public Task<AuthResponse> Login(LoginRequest request, CancellationToken cancellationToken) =>
        authService.LoginAsync(request, cancellationToken);

    [HttpPost("refresh")]
    [AllowAnonymous]
    public Task<AuthResponse> Refresh(RefreshRequest request, CancellationToken cancellationToken) =>
        authService.RefreshAsync(request, cancellationToken);

    [HttpPost("logout")]
    [Authorize]
    public IActionResult Logout() => NoContent();
}
