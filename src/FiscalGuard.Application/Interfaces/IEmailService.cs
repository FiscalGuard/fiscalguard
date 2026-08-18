namespace FiscalGuard.Application;

public interface IEmailService
{
    Task SendAsync(string recipient, string subject, string content, CancellationToken cancellationToken);
}
