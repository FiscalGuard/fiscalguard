using System.Diagnostics;
using FiscalGuard.Application;
using FiscalGuard.Domain;
using Microsoft.EntityFrameworkCore;

namespace FiscalGuard.Infrastructure.Services;

public sealed class FiscalMonitoringService(
    FiscalGuardDbContext db,
    ICurrentUserContext currentUser,
    IFiscalDataProvider provider) : IFiscalMonitoringService
{
    public async Task<FiscalConsultationResult> ConsultCompanyAsync(Guid companyId, CancellationToken cancellationToken)
    {
        var company = await db.Companies.SingleAsync(
            x => x.Id == companyId && x.OrganizationId == currentUser.OrganizationId,
            cancellationToken);

        if (company.FiscalStatus == FiscalStatus.Processing)
        {
            throw new InvalidOperationException("Consulta ja em processamento para este CNPJ.");
        }

        var previous = company.FiscalStatus;
        company.FiscalStatus = FiscalStatus.Processing;
        await db.SaveChangesAsync(cancellationToken);

        var stopwatch = Stopwatch.StartNew();
        var result = await provider.ConsultAsync(company.Cnpj, cancellationToken);
        stopwatch.Stop();

        var consultation = CreateConsultation(company, result, stopwatch.ElapsedMilliseconds);
        db.FiscalConsultations.Add(consultation);

        UpdateCompanyStatus(company, result);
        db.FiscalStatusHistory.Add(new FiscalStatusHistory
        {
            OrganizationId = currentUser.OrganizationId,
            CompanyId = company.Id,
            PreviousStatus = previous,
            NewStatus = result.Status,
            Reason = result.Error
        });

        foreach (var finding in result.Findings)
        {
            await UpsertIssueAsync(company, consultation, finding, cancellationToken);
        }

        await db.SaveChangesAsync(cancellationToken);
        return result;
    }

    private FiscalConsultation CreateConsultation(
        Company company,
        FiscalConsultationResult result,
        long durationMs) =>
        new()
        {
            OrganizationId = currentUser.OrganizationId,
            CompanyId = company.Id,
            Provider = result.Provider,
            Success = result.Success,
            NormalizedStatus = result.Status,
            RawPayloadProtected = result.RawPayloadProtected,
            Error = result.Error,
            DurationMs = durationMs
        };

    private static void UpdateCompanyStatus(Company company, FiscalConsultationResult result)
    {
        company.FiscalStatus = result.Status;
        company.LastConsultedAt = DateTimeOffset.UtcNow;
        company.NextConsultationAt = DateTimeOffset.UtcNow.AddDays((int)company.MonitoringFrequency == 0 ? 7 : (int)company.MonitoringFrequency);
    }

    private async Task UpsertIssueAsync(
        Company company,
        FiscalConsultation consultation,
        FiscalFinding finding,
        CancellationToken cancellationToken)
    {
        var key = BuildDeduplicationKey(company.Id, finding.ExternalKey);
        var issue = await db.FiscalIssues.SingleOrDefaultAsync(
            x => x.OrganizationId == currentUser.OrganizationId && x.DeduplicationKey == key,
            cancellationToken);

        if (issue is null)
        {
            issue = new FiscalIssue
            {
                OrganizationId = currentUser.OrganizationId,
                CompanyId = company.Id,
                Type = finding.Type,
                Title = finding.Title,
                Description = finding.Description,
                Severity = finding.Severity,
                ExternalIdentifier = finding.ExternalKey,
                DeduplicationKey = key
            };
            db.FiscalIssues.Add(issue);
            AddAlert(company, issue, AlertType.NewIssue, "Nova pendencia fiscal", finding);
        }
        else
        {
            ReopenIssueIfNeeded(company, issue, finding);
            issue.UpdatedAt = DateTimeOffset.UtcNow;
            issue.Severity = finding.Severity > issue.Severity ? finding.Severity : issue.Severity;
        }

        db.FiscalConsultationItems.Add(new FiscalConsultationItem
        {
            OrganizationId = currentUser.OrganizationId,
            FiscalConsultation = consultation,
            ExternalKey = finding.ExternalKey,
            Title = finding.Title,
            Description = finding.Description,
            Severity = finding.Severity
        });
    }

    private void ReopenIssueIfNeeded(Company company, FiscalIssue issue, FiscalFinding finding)
    {
        if (issue.Status != IssueStatus.Resolved)
        {
            return;
        }

        var previousStatus = issue.Status;
        issue.Status = IssueStatus.Reopened;
        db.FiscalIssueHistory.Add(new FiscalIssueHistory
        {
            OrganizationId = currentUser.OrganizationId,
            FiscalIssueId = issue.Id,
            PreviousStatus = previousStatus,
            NewStatus = IssueStatus.Reopened,
            Notes = "Irregularidade reapareceu em nova consulta."
        });
        AddAlert(company, issue, AlertType.IssueReopened, "Pendencia reaberta", finding);
    }

    private void AddAlert(
        Company company,
        FiscalIssue issue,
        AlertType type,
        string title,
        FiscalFinding finding)
    {
        db.Alerts.Add(new Alert
        {
            OrganizationId = currentUser.OrganizationId,
            CompanyId = company.Id,
            FiscalIssueId = issue.Id,
            Type = type,
            Severity = finding.Severity,
            Title = title,
            Message = $"{company.LegalName}: {finding.Title}"
        });
    }

    private static string BuildDeduplicationKey(Guid companyId, string externalKey) =>
        $"{companyId}:{externalKey}".ToLowerInvariant();
}
