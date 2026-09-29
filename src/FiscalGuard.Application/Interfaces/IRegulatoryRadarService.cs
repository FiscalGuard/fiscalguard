namespace FiscalGuard.Application;

public interface IRegulatoryRadarService
{
    Task<RegulatoryRadarSummary> GetLatestAsync(bool forceRefresh, CancellationToken cancellationToken);
}
