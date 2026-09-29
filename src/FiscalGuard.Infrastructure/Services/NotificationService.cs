using FiscalGuard.Application;
using FiscalGuard.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FiscalGuard.Infrastructure.Services;

public sealed class NotificationService(
    FiscalGuardDbContext db,
    IEmailService emailService,
    ILogger<NotificationService> logger) : INotificationService
{
    public async Task CreateForAlertAsync(Alert alert, CancellationToken cancellationToken)
    {
        var recipients = await db.OrganizationUsers
            .AsNoTracking()
            .Where(x => x.OrganizationId == alert.OrganizationId && x.IsActive && x.User!.IsActive)
            .Select(x => x.User!.Email)
            .Distinct()
            .ToListAsync(cancellationToken);

        foreach (var recipient in recipients)
        {
            AddNotification(alert, recipient, NotificationChannel.Internal, NotificationStatus.Sent);
        }

        var emailAlertsEnabled = await db.Subscriptions
            .AsNoTracking()
            .Where(x => x.OrganizationId == alert.OrganizationId)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => x.Plan != null && x.Plan.EmailAlerts)
            .FirstOrDefaultAsync(cancellationToken);

        if (!emailAlertsEnabled)
        {
            return;
        }

        foreach (var recipient in recipients)
        {
            var notification = AddNotification(alert, recipient, NotificationChannel.Email, NotificationStatus.Pending);
            try
            {
                await emailService.SendAsync(recipient, alert.Title, alert.Message, cancellationToken);
                notification.Status = NotificationStatus.Sent;
                notification.SentAt = DateTimeOffset.UtcNow;
            }
            catch (Exception ex)
            {
                notification.Status = NotificationStatus.Failed;
                notification.Error = ex.Message;
                logger.LogWarning(ex, "Failed to send alert email to {Recipient}", recipient);
            }
            finally
            {
                notification.Attempts++;
            }
        }
    }

    private Notification AddNotification(
        Alert alert,
        string recipient,
        NotificationChannel channel,
        NotificationStatus status)
    {
        var notification = new Notification
        {
            OrganizationId = alert.OrganizationId,
            AlertId = alert.Id,
            FiscalIssueId = alert.FiscalIssueId,
            Recipient = recipient,
            Channel = channel,
            Content = $"{alert.Title}: {alert.Message}",
            Status = status,
            SentAt = status == NotificationStatus.Sent ? DateTimeOffset.UtcNow : null
        };

        db.Notifications.Add(notification);
        return notification;
    }
}
