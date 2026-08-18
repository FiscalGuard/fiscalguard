namespace FiscalGuard.Application;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterOrganizationRequest request, CancellationToken cancellationToken);
    Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken);
    Task<AuthResponse> RefreshAsync(RefreshRequest request, CancellationToken cancellationToken);
}
