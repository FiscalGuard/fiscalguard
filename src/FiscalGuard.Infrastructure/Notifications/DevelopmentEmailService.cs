using FiscalGuard.Application;
using Microsoft.Extensions.Logging;

namespace FiscalGuard.Infrastructure.Notifications;

public sealed class DevelopmentEmailService(ILogger<DevelopmentEmailService> logger) : IEmailService
{
    public Task SendAsync(string recipient, string subject, string content, CancellationToken cancellationToken)
    {
        logger.LogInformation("Dev email to {Recipient}: {Subject}", recipient, subject);
        return Task.CompletedTask;
    }
}
