using FiscalGuard.Domain;

namespace FiscalGuard.Application;

public interface INotificationService
{
    Task CreateForAlertAsync(Alert alert, CancellationToken cancellationToken);
}
