using FiscalGuard.Application;
using FiscalGuard.Domain;
using Microsoft.EntityFrameworkCore;

namespace FiscalGuard.Infrastructure.Services;

public sealed class DashboardService(FiscalGuardDbContext db, ICurrentUserContext currentUser) : IDashboardService
{
    public async Task<DashboardSummary> GetAsync(CancellationToken cancellationToken)
    {
        var organizationId = currentUser.OrganizationId;
        var subscription = await db.Subscriptions
            .Include(x => x.Plan)
            .SingleAsync(x => x.OrganizationId == organizationId, cancellationToken);

        return new DashboardSummary(
            await CountCompaniesAsync(organizationId, null, cancellationToken),
            await CountCompaniesAsync(organizationId, FiscalStatus.Regular, cancellationToken),
            await CountCompaniesAsync(organizationId, FiscalStatus.Attention, cancellationToken),
            await CountCompaniesAsync(organizationId, FiscalStatus.Irregular, cancellationToken),
            await db.FiscalIssues.CountAsync(x => x.OrganizationId == organizationId && x.Status == IssueStatus.Open, cancellationToken),
            await db.Alerts.CountAsync(x => x.OrganizationId == organizationId && !x.IsRead, cancellationToken),
            await db.FiscalConsultations.CountAsync(x => x.OrganizationId == organizationId, cancellationToken),
            await db.Companies.CountAsync(x => x.OrganizationId == organizationId && x.Status == CompanyStatus.Active && x.RiskLevel == "Alto", cancellationToken),
            await db.Companies.CountAsync(x => x.OrganizationId == organizationId && x.Status == CompanyStatus.Active && x.RiskLevel == "Critico", cancellationToken),
            await CountCompaniesAsync(organizationId, FiscalStatus.NotConsulted, cancellationToken),
            await CountCompaniesAsync(organizationId, FiscalStatus.QueryError, cancellationToken),
            await db.Companies.CountAsync(x => x.OrganizationId == organizationId && x.Status == CompanyStatus.Active && x.PublicDataUpdatedAt != null, cancellationToken),
            await db.Companies.CountAsync(x => x.OrganizationId == organizationId && x.Status == CompanyStatus.Active && x.IsSimplesOption == true, cancellationToken),
            await db.Companies.CountAsync(x => x.OrganizationId == organizationId && x.Status == CompanyStatus.Active && x.IsSimplesOption == false, cancellationToken),
            await db.FiscalIssues.CountAsync(x => x.OrganizationId == organizationId && x.Recommendation != null, cancellationToken),
            await db.Companies
                .Where(x => x.OrganizationId == organizationId && x.Status == CompanyStatus.Active)
                .OrderByDescending(x => x.RiskScore)
                .ThenByDescending(x => x.LastConsultedAt)
                .Take(5)
                .Select(x => new DashboardRiskCompany(
                    x.Id,
                    x.LegalName,
                    x.Cnpj,
                    x.FiscalStatus,
                    x.RiskScore,
                    x.RiskLevel,
                    db.FiscalIssues.Count(i => i.OrganizationId == organizationId && i.CompanyId == x.Id && i.Status == IssueStatus.Open)))
                .ToListAsync(cancellationToken),
            await BuildValueIndicatorsAsync(organizationId, cancellationToken),
            subscription.Plan!.Name,
            subscription.Plan.CnpjLimit,
            CalculateDaysRemaining(subscription.TrialEndsAt));
    }

    private Task<int> CountCompaniesAsync(Guid organizationId, FiscalStatus? fiscalStatus, CancellationToken cancellationToken)
    {
        var query = db.Companies.Where(x => x.OrganizationId == organizationId && x.Status == CompanyStatus.Active);
        if (fiscalStatus.HasValue)
        {
            query = query.Where(x => x.FiscalStatus == fiscalStatus.Value);
        }

        return query.CountAsync(cancellationToken);
    }

    private static int CalculateDaysRemaining(DateTimeOffset? trialEnd) =>
        trialEnd is null ? 0 : Math.Max(0, (int)Math.Ceiling((trialEnd.Value - DateTimeOffset.UtcNow).TotalDays));

    private async Task<IReadOnlyList<DashboardValueIndicator>> BuildValueIndicatorsAsync(Guid organizationId, CancellationToken cancellationToken)
    {
        var activeCompanies = await db.Companies.CountAsync(x => x.OrganizationId == organizationId && x.Status == CompanyStatus.Active, cancellationToken);
        var publicData = await db.Companies.CountAsync(x => x.OrganizationId == organizationId && x.Status == CompanyStatus.Active && x.PublicDataUpdatedAt != null, cancellationToken);
        var optIn = await db.Companies.CountAsync(x => x.OrganizationId == organizationId && x.Status == CompanyStatus.Active && x.IsSimplesOption == true, cancellationToken);
        var openIssues = await db.FiscalIssues.CountAsync(x => x.OrganizationId == organizationId && x.Status == IssueStatus.Open, cancellationToken);
        var recommendations = await db.FiscalIssues.CountAsync(x => x.OrganizationId == organizationId && x.Recommendation != null, cancellationToken);

        return
        [
            new(
                "Carteira enriquecida",
                $"{publicData}/{activeCompanies}",
                "CNPJs com dados publicos consolidados para decisao do escritorio."),
            new(
                "Enquadramento Simples",
                optIn.ToString(),
                "Empresas identificadas como optantes quando a fonte publica informa o indicador."),
            new(
                "Acoes recomendadas",
                recommendations.ToString(),
                "Orientacoes geradas para acelerar a triagem e o contato com o cliente."),
            new(
                "Pendencias priorizadas",
                openIssues.ToString(),
                "Itens abertos organizados por severidade e score de risco.")
        ];
    }
}
