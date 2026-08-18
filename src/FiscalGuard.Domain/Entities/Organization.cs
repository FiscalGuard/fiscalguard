namespace FiscalGuard.Domain;

public sealed class Organization : Entity
{
    public required string Name { get; set; }
    public string? Document { get; set; }
    public string? Phone { get; set; }
    public bool IsActive { get; set; } = true;
}
