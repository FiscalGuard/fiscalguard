namespace FiscalGuard.Domain;

public sealed class AppUser : Entity
{
    public required string Name { get; set; }
    public required string Email { get; set; }
    public required string PasswordHash { get; set; }
    public string? Phone { get; set; }
    public bool IsActive { get; set; } = true;
}
