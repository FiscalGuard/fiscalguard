namespace FiscalGuard.Application;

public interface IDashboardService
{
    Task<DashboardSummary> GetAsync(CancellationToken cancellationToken);
}
