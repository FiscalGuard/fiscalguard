namespace FiscalGuard.Application;

public interface IAlertService
{
    Task<IReadOnlyList<AlertDto>> ListAsync(CancellationToken cancellationToken);
    Task MarkReadAsync(Guid alertId, CancellationToken cancellationToken);
}
