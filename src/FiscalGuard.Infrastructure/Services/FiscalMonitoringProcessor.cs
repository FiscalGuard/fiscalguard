using System.Diagnostics;
using FiscalGuard.Application;
using FiscalGuard.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FiscalGuard.Infrastructure.Services;

public sealed class FiscalMonitoringProcessor(
    FiscalGuardDbContext db,
    IFiscalDataProvider provider,
    INotificationService notificationService,
    ILogger<FiscalMonitoringProcessor> logger) : IFiscalMonitoringProcessor
{
    public async Task<FiscalConsultationResult> ProcessAsync(
        Guid organizationId,
        Guid companyId,
        Guid? requestedByUserId,
        CancellationToken cancellationToken)
    {
        var company = await db.Companies
            .Include(x => x.Customer)
            .SingleAsync(x => x.Id == companyId && x.OrganizationId == organizationId, cancellationToken);

        if (company.FiscalStatus == FiscalStatus.Processing)
        {
            throw new InvalidOperationException("Consulta ja em processamento para este CNPJ.");
        }

        var previous = company.FiscalStatus;
        company.FiscalStatus = FiscalStatus.Processing;
        await db.SaveChangesAsync(cancellationToken);

        var stopwatch = Stopwatch.StartNew();
        var result = await ConsultProviderAsync(company, cancellationToken);
        stopwatch.Stop();

        var consultation = CreateConsultation(organizationId, company, result, stopwatch.ElapsedMilliseconds);
        db.FiscalConsultations.Add(consultation);

        ApplyCompanySnapshot(company, result.CompanySnapshot);
        UpdateCompanyStatus(company, result);
        await UpdateRiskAsync(organizationId, company, result.Findings, cancellationToken);
        db.FiscalStatusHistory.Add(new FiscalStatusHistory
        {
            OrganizationId = organizationId,
            CompanyId = company.Id,
            PreviousStatus = previous,
            NewStatus = result.Status,
            Reason = result.Error
        });

        foreach (var finding in result.Findings)
        {
            await UpsertIssueAsync(organizationId, company, consultation, finding, cancellationToken);
        }

        await db.SaveChangesAsync(cancellationToken);
        logger.LogInformation(
            "Fiscal consultation processed for company {CompanyId} in organization {OrganizationId} by {UserId}",
            company.Id,
            organizationId,
            requestedByUserId);

        return result;
    }

    private async Task<FiscalConsultationResult> ConsultProviderAsync(Company company, CancellationToken cancellationToken)
    {
        try
        {
            return await provider.ConsultAsync(company.Cnpj, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Fiscal provider failed for company {CompanyId}", company.Id);
            return new FiscalConsultationResult(
                "system",
                FiscalStatus.QueryError,
                false,
                $$"""{"error":"{{ex.GetType().Name}}"}""",
                ex.Message,
                [
                    new FiscalFinding(
                        "provider-exception",
                        IssueType.IntegrationFailure,
                        "Falha na consulta fiscal",
                        "O provedor fiscal nao respondeu corretamente durante a consulta automatica.",
                        IssueSeverity.Low)
                ]);
        }
    }

    private static FiscalConsultation CreateConsultation(
        Guid organizationId,
        Company company,
        FiscalConsultationResult result,
        long durationMs) =>
        new()
        {
            OrganizationId = organizationId,
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
        var now = DateTimeOffset.UtcNow;
        company.FiscalStatus = result.Status;
        company.LastConsultedAt = now;
        company.NextConsultationAt = company.MonitoringFrequency == MonitoringFrequency.Manual
            ? null
            : now.AddDays((int)company.MonitoringFrequency);
    }

    private static void ApplyCompanySnapshot(Company company, FiscalCompanySnapshot? snapshot)
    {
        if (snapshot is null)
        {
            return;
        }

        if (!string.IsNullOrWhiteSpace(snapshot.LegalName))
        {
            company.LegalName = snapshot.LegalName;
            if (company.Customer is not null)
            {
                company.Customer.LegalName = snapshot.LegalName;
            }
        }

        if (!string.IsNullOrWhiteSpace(snapshot.TradeName))
        {
            company.TradeName = snapshot.TradeName;
            if (company.Customer is not null)
            {
                company.Customer.TradeName = snapshot.TradeName;
            }
        }

        company.RegistrationStatus = snapshot.RegistrationStatus;
        company.RegistrationStatusDate = snapshot.RegistrationStatusDate;
        company.MainCnaeCode = snapshot.MainCnaeCode;
        company.MainCnaeDescription = snapshot.MainCnaeDescription;
        company.PublicAddress = snapshot.PublicAddress;
        company.PublicDataSource = snapshot.PublicDataSource;
        company.PublicDataUpdatedAt = snapshot.PublicDataUpdatedAt;
        company.IsSimplesOption = snapshot.IsSimplesOption;
        company.IsMeiOption = snapshot.IsMeiOption;

        if (snapshot.IsMeiOption is true)
        {
            company.TaxRegime = TaxRegime.Mei;
        }
        else if (snapshot.IsSimplesOption is true)
        {
            company.TaxRegime = TaxRegime.SimplesNacional;
        }
    }

    private async Task UpdateRiskAsync(
        Guid organizationId,
        Company company,
        IReadOnlyList<FiscalFinding> currentFindings,
        CancellationToken cancellationToken)
    {
        var openIssues = await db.FiscalIssues
            .Where(x => x.OrganizationId == organizationId && x.CompanyId == company.Id && x.Status == IssueStatus.Open)
            .Select(x => x.Severity)
            .ToListAsync(cancellationToken);

        var severities = openIssues.Concat(currentFindings.Select(x => x.Severity)).ToList();
        var score = 10;

        score += severities.Count(x => x == IssueSeverity.Critical) * 35;
        score += severities.Count(x => x == IssueSeverity.High) * 25;
        score += severities.Count(x => x == IssueSeverity.Medium) * 12;
        score += severities.Count(x => x == IssueSeverity.Low) * 6;

        if (company.RegistrationStatus is not null && !company.RegistrationStatus.Equals("ATIVA", StringComparison.OrdinalIgnoreCase))
        {
            score += 25;
        }

        if (company.IsSimplesOption is false)
        {
            score += 20;
        }

        if (company.LastConsultedAt is null || company.LastConsultedAt < DateTimeOffset.UtcNow.AddDays(-30))
        {
            score += 8;
        }

        company.RiskScore = Math.Clamp(score, 0, 100);
        company.RiskLevel = company.RiskScore switch
        {
            >= 80 => "Critico",
            >= 60 => "Alto",
            >= 35 => "Medio",
            _ => "Baixo"
        };
        company.RiskSummary = company.RiskLevel switch
        {
            "Critico" => "Risco critico: priorizar atendimento e validar as fontes oficiais antes de novas operacoes.",
            "Alto" => "Risco alto: existem sinais relevantes que exigem acao do contador.",
            "Medio" => "Risco medio: acompanhar e revisar os pontos sinalizados na proxima rotina.",
            _ => "Risco baixo: dados publicos nao indicam irregularidade relevante neste momento."
        };
    }

    private async Task UpsertIssueAsync(
        Guid organizationId,
        Company company,
        FiscalConsultation consultation,
        FiscalFinding finding,
        CancellationToken cancellationToken)
    {
        var key = BuildDeduplicationKey(company.Id, finding.ExternalKey);
        var issue = await db.FiscalIssues.SingleOrDefaultAsync(
            x => x.OrganizationId == organizationId && x.DeduplicationKey == key,
            cancellationToken);

        if (issue is null)
        {
            issue = new FiscalIssue
            {
                OrganizationId = organizationId,
                CompanyId = company.Id,
                Type = finding.Type,
                Title = finding.Title,
                Description = finding.Description,
                Severity = finding.Severity,
                Recommendation = finding.Recommendation,
                EvidenceType = finding.EvidenceType,
                ExternalIdentifier = finding.ExternalKey,
                DeduplicationKey = key
            };
            db.FiscalIssues.Add(issue);
            await AddAlertAsync(organizationId, company, issue, AlertType.NewIssue, "Nova pendencia fiscal", finding, cancellationToken);
        }
        else
        {
            await ReopenIssueIfNeededAsync(organizationId, company, issue, finding, cancellationToken);
            issue.UpdatedAt = DateTimeOffset.UtcNow;
            issue.Severity = finding.Severity > issue.Severity ? finding.Severity : issue.Severity;
            issue.Description = finding.Description;
            issue.Recommendation = finding.Recommendation;
            issue.EvidenceType = finding.EvidenceType;
        }

        db.FiscalConsultationItems.Add(new FiscalConsultationItem
        {
            OrganizationId = organizationId,
            FiscalConsultation = consultation,
            ExternalKey = finding.ExternalKey,
            Title = finding.Title,
            Description = finding.Description,
            Severity = finding.Severity
        });
    }

    private async Task ReopenIssueIfNeededAsync(
        Guid organizationId,
        Company company,
        FiscalIssue issue,
        FiscalFinding finding,
        CancellationToken cancellationToken)
    {
        if (issue.Status != IssueStatus.Resolved)
        {
            return;
        }

        var previousStatus = issue.Status;
        issue.Status = IssueStatus.Reopened;
        db.FiscalIssueHistory.Add(new FiscalIssueHistory
        {
            OrganizationId = organizationId,
            FiscalIssueId = issue.Id,
            PreviousStatus = previousStatus,
            NewStatus = IssueStatus.Reopened,
            Notes = "Irregularidade reapareceu em nova consulta."
        });
        await AddAlertAsync(organizationId, company, issue, AlertType.IssueReopened, "Pendencia reaberta", finding, cancellationToken);
    }

    private async Task AddAlertAsync(
        Guid organizationId,
        Company company,
        FiscalIssue issue,
        AlertType type,
        string title,
        FiscalFinding finding,
        CancellationToken cancellationToken)
    {
        var alert = new Alert
        {
            OrganizationId = organizationId,
            CompanyId = company.Id,
            FiscalIssueId = issue.Id,
            Type = type,
            Severity = finding.Severity,
            Title = title,
            Message = $"{company.LegalName}: {finding.Title}"
        };

        db.Alerts.Add(alert);
        await notificationService.CreateForAlertAsync(alert, cancellationToken);
    }

    private static string BuildDeduplicationKey(Guid companyId, string externalKey) =>
        $"{companyId}:{externalKey}".ToLowerInvariant();
}
