using FiscalGuard.Domain;
using Microsoft.EntityFrameworkCore;

namespace FiscalGuard.Infrastructure.Seed;

public sealed class DevelopmentSeeder(FiscalGuardDbContext db, PasswordHasher hasher)
{
    public async Task SeedAsync(CancellationToken cancellationToken)
    {
        if (await db.Organizations.AnyAsync(cancellationToken))
        {
            await UpdateExistingDemoDataAsync(cancellationToken);
            return;
        }

        var trial = await db.Plans.SingleAsync(x => x.Code == "trial", cancellationToken);
        var organization = new Organization { Name = "Contabilidade Guard Demo", Document = "11222333000181", Phone = "(11) 4002-8922" };
        var owner = new AppUser { Name = "Rayssa Demo", Email = "demo@fiscalguard.local", PasswordHash = hasher.Hash("Demo@12345") };
        db.Organizations.Add(organization);
        db.Users.Add(owner);
        db.OrganizationUsers.Add(new OrganizationUser { Organization = organization, User = owner, Role = UserRole.Owner });
        db.Subscriptions.Add(new Subscription { OrganizationId = organization.Id, Plan = trial, Status = SubscriptionStatus.Trial, TrialEndsAt = DateTimeOffset.UtcNow.AddDays(14) });
        db.SubscriptionUsage.Add(new SubscriptionUsage { OrganizationId = organization.Id, ActiveUsersCount = 1 });

        var companies = new[]
        {
            new DemoCompany("Mercado Regular Ltda", "11222333000181", FiscalStatus.Regular, 12, "Baixo", null, null, null),
            new DemoCompany(
                "Clinica Atencao Fiscal",
                "04252011000110",
                FiscalStatus.Attention,
                46,
                "Medio",
                IssueSeverity.Medium,
                "Indicador requer revisao",
                "A fonte publica nao trouxe todos os dados esperados para validar o enquadramento com seguranca."),
            new DemoCompany(
                "Construtora Risco Alto",
                "34235753000104",
                FiscalStatus.Irregular,
                78,
                "Alto",
                IssueSeverity.High,
                "Empresa nao consta como optante pelo Simples",
                "A fonte publica de demonstracao indica divergencia relevante para o publico-alvo do FiscalGuard.")
        };

        foreach (var item in companies)
        {
            var customer = new Customer { OrganizationId = organization.Id, LegalName = item.LegalName, Email = "cliente@exemplo.local", ResponsibleName = "Responsavel Fiscal" };
            var company = new Company
            {
                OrganizationId = organization.Id,
                Customer = customer,
                LegalName = item.LegalName,
                Cnpj = item.Cnpj,
                TaxRegime = TaxRegime.SimplesNacional,
                FiscalStatus = item.FiscalStatus,
                RegistrationStatus = "ATIVA",
                RegistrationStatusDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-3)),
                MainCnaeCode = "6920601",
                MainCnaeDescription = "Atividades de contabilidade",
                PublicAddress = "Avenida Paulista, 1000, Sao Paulo, SP",
                PublicDataSource = "demo-seed",
                PublicDataUpdatedAt = DateTimeOffset.UtcNow.AddHours(-4),
                IsSimplesOption = item.FiscalStatus != FiscalStatus.Irregular,
                IsMeiOption = false,
                RiskScore = item.RiskScore,
                RiskLevel = item.RiskLevel,
                RiskSummary = BuildRiskSummary(item.RiskLevel),
                LastConsultedAt = DateTimeOffset.UtcNow.AddHours(-4),
                NextConsultationAt = DateTimeOffset.UtcNow.AddDays(3)
            };
            db.Customers.Add(customer);
            db.Companies.Add(company);
            db.FiscalConsultations.Add(new FiscalConsultation
            {
                OrganizationId = organization.Id,
                Company = company,
                Provider = "demo-seed",
                Success = true,
                NormalizedStatus = item.FiscalStatus,
                RawPayloadProtected = $$"""{"source":"demo-seed","cnpj":"{{item.Cnpj}}"}""",
                DurationMs = 120
            });

            if (item.FiscalStatus != FiscalStatus.Regular && item.IssueSeverity.HasValue)
            {
                var issue = new FiscalIssue
                {
                    OrganizationId = organization.Id,
                    Company = company,
                    Type = IssueType.MissingFiling,
                    Title = item.IssueTitle!,
                    Description = item.IssueDescription!,
                    Severity = item.IssueSeverity.Value,
                    Recommendation = item.FiscalStatus == FiscalStatus.Irregular
                        ? "Priorizar contato com o cliente e validar o enquadramento no Portal do Simples Nacional antes de novas orientacoes."
                        : "Conferir os dados cadastrais publicos e registrar a revisao no acompanhamento do cliente.",
                    EvidenceType = "Dado de demonstracao",
                    DeduplicationKey = $"{company.Id}:seed-issue"
                };
                db.FiscalIssues.Add(issue);
                db.Alerts.Add(new Alert
                {
                    OrganizationId = organization.Id,
                    CompanyId = company.Id,
                    FiscalIssueId = issue.Id,
                    Type = AlertType.NewIssue,
                    Severity = item.IssueSeverity.Value,
                    Title = "Pendencia detectada",
                    Message = $"{item.LegalName} possui item para revisao."
                });
            }
        }

        db.AuditLogs.Add(new AuditLog { OrganizationId = organization.Id, UserId = owner.Id, Action = "seed.created", EntityName = "DevelopmentSeed" });
        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task UpdateExistingDemoDataAsync(CancellationToken cancellationToken)
    {
        var organization = await db.Organizations
            .Where(x => x.Name == "Contabilidade Guard Demo")
            .FirstOrDefaultAsync(cancellationToken);

        if (organization is null)
        {
            return;
        }

        var companies = await db.Companies
            .Where(x => x.OrganizationId == organization.Id)
            .ToListAsync(cancellationToken);

        foreach (var company in companies)
        {
            company.RegistrationStatus ??= "ATIVA";
            company.RegistrationStatusDate ??= DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-3));
            company.MainCnaeCode ??= "6920601";
            company.MainCnaeDescription ??= "Atividades de contabilidade";
            company.PublicAddress ??= "Avenida Paulista, 1000, Sao Paulo, SP";
            company.PublicDataSource ??= "demo-seed";
            company.PublicDataUpdatedAt ??= DateTimeOffset.UtcNow.AddHours(-4);
            company.IsSimplesOption ??= company.FiscalStatus != FiscalStatus.Irregular;
            company.IsMeiOption ??= false;

            var risk = company.FiscalStatus switch
            {
                FiscalStatus.Irregular => (78, "Alto"),
                FiscalStatus.Attention => (46, "Medio"),
                FiscalStatus.Regular => (12, "Baixo"),
                _ => (company.RiskScore == 0 ? 20 : company.RiskScore, string.IsNullOrWhiteSpace(company.RiskLevel) ? "Nao avaliado" : company.RiskLevel)
            };
            company.RiskScore = company.RiskScore == 0 ? risk.Item1 : company.RiskScore;
            company.RiskLevel = string.IsNullOrWhiteSpace(company.RiskLevel) ? risk.Item2 : company.RiskLevel;
            company.RiskSummary ??= BuildRiskSummary(company.RiskLevel);

            var hasConsultation = await db.FiscalConsultations.AnyAsync(
                x => x.OrganizationId == organization.Id && x.CompanyId == company.Id,
                cancellationToken);
            if (!hasConsultation)
            {
                db.FiscalConsultations.Add(new FiscalConsultation
                {
                    OrganizationId = organization.Id,
                    CompanyId = company.Id,
                    Provider = "demo-seed",
                    Success = true,
                    NormalizedStatus = company.FiscalStatus,
                    RawPayloadProtected = $$"""{"source":"demo-seed","cnpj":"{{company.Cnpj}}"}""",
                    DurationMs = 120
                });
            }
        }

        var issues = await db.FiscalIssues
            .Where(x => x.OrganizationId == organization.Id)
            .ToListAsync(cancellationToken);

        foreach (var issue in issues)
        {
            issue.EvidenceType = string.IsNullOrWhiteSpace(issue.EvidenceType) ? "Dado de demonstracao" : issue.EvidenceType;
            issue.Recommendation ??= issue.Severity >= IssueSeverity.High
                ? "Priorizar contato com o cliente e validar a situacao no portal oficial antes de novas orientacoes."
                : "Conferir os dados publicos e registrar a revisao no acompanhamento do cliente.";
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    private static string BuildRiskSummary(string riskLevel) =>
        riskLevel switch
        {
            "Alto" => "Risco alto: existem sinais relevantes que exigem acao do contador.",
            "Medio" => "Risco medio: acompanhar e revisar os pontos sinalizados na proxima rotina.",
            "Baixo" => "Risco baixo: dados publicos nao indicam irregularidade relevante neste momento.",
            _ => "Empresa aguardando consolidacao completa dos dados de monitoramento."
        };

    private sealed record DemoCompany(
        string LegalName,
        string Cnpj,
        FiscalStatus FiscalStatus,
        int RiskScore,
        string RiskLevel,
        IssueSeverity? IssueSeverity,
        string? IssueTitle,
        string? IssueDescription);
}
