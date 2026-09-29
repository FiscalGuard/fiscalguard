using System.Net;
using System.Net.Mail;
using FiscalGuard.Application;
using Microsoft.Extensions.Configuration;

namespace FiscalGuard.Infrastructure.Notifications;

public sealed class SmtpEmailService(IConfiguration configuration) : IEmailService
{
    public async Task SendAsync(string recipient, string subject, string content, CancellationToken cancellationToken)
    {
        var host = configuration["Smtp:Host"] ?? throw new InvalidOperationException("SMTP host nao configurado.");
        var port = int.TryParse(configuration["Smtp:Port"], out var configuredPort) ? configuredPort : 587;
        var from = configuration["Smtp:From"] ?? configuration["Smtp:Username"] ?? throw new InvalidOperationException("SMTP remetente nao configurado.");
        var username = configuration["Smtp:Username"];
        var password = configuration["Smtp:Password"];
        var enableSsl = !bool.TryParse(configuration["Smtp:EnableSsl"], out var ssl) || ssl;

        using var message = new MailMessage(from, recipient, subject, content)
        {
            IsBodyHtml = false
        };
        using var client = new SmtpClient(host, port)
        {
            EnableSsl = enableSsl
        };

        if (!string.IsNullOrWhiteSpace(username))
        {
            client.Credentials = new NetworkCredential(username, password);
        }

        await client.SendMailAsync(message, cancellationToken);
    }
}
