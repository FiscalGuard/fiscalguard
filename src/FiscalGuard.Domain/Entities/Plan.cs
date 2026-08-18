namespace FiscalGuard.Domain;

public sealed class Plan : Entity
{
    public required string Code { get; set; }
    public required string Name { get; set; }
    public int CnpjLimit { get; set; }
    public int UserLimit { get; set; }
    public int TrialDays { get; set; }
    public bool AutomatedMonitoring { get; set; }
    public bool EmailAlerts { get; set; }
    public bool FullHistory { get; set; }
}
