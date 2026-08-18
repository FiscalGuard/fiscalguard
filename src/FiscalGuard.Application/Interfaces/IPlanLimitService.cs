namespace FiscalGuard.Application;

public interface IPlanLimitService
{
    Task EnsureCanAddCompanyAsync(Guid organizationId, CancellationToken cancellationToken);
}
