using FiscalGuard.Domain;
using Microsoft.EntityFrameworkCore;

namespace FiscalGuard.Infrastructure.Seed;

public sealed class DevelopmentSeeder(FiscalGuardDbContext db, PasswordHasher hasher)
{
    public async Task SeedAsync(CancellationToken cancellationToken)
    {
        if (await db.Organizations.AnyAsync(cancellationToken))
        {
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
            ("Mercado Regular Ltda", "11222333000181", FiscalStatus.Regular),
            ("Clinica Atencao Fiscal", "04252011000110", FiscalStatus.Attention),
            ("Construtora Risco Alto", "34235753000104", FiscalStatus.Irregular)
        };

        foreach (var item in companies)
        {
            var customer = new Customer { OrganizationId = organization.Id, LegalName = item.Item1, Email = "cliente@exemplo.local", ResponsibleName = "Responsavel Fiscal" };
            var company = new Company
            {
                OrganizationId = organization.Id,
                Customer = customer,
                LegalName = item.Item1,
                Cnpj = item.Item2,
                TaxRegime = TaxRegime.SimplesNacional,
                FiscalStatus = item.Item3,
                LastConsultedAt = DateTimeOffset.UtcNow.AddHours(-4),
                NextConsultationAt = DateTimeOffset.UtcNow.AddDays(3)
            };
            db.Customers.Add(customer);
            db.Companies.Add(company);

            if (item.Item3 != FiscalStatus.Regular)
            {
                var severity = item.Item3 == FiscalStatus.Irregular ? IssueSeverity.High : IssueSeverity.Medium;
                db.FiscalIssues.Add(new FiscalIssue
                {
                    OrganizationId = organization.Id,
                    Company = company,
                    Type = IssueType.MissingFiling,
                    Title = item.Item3 == FiscalStatus.Irregular ? "Declaracao pendente" : "Indicador requer revisao",
                    Description = "Exemplo de demonstracao para evidenciar risco fiscal preventivo.",
                    Severity = severity,
                    DeduplicationKey = $"{company.Id}:seed-issue"
                });
                db.Alerts.Add(new Alert
                {
                    OrganizationId = organization.Id,
                    CompanyId = company.Id,
                    Type = AlertType.NewIssue,
                    Severity = severity,
                    Title = "Pendencia detectada",
                    Message = $"{item.Item1} possui item para revisao."
                });
            }
        }

        db.AuditLogs.Add(new AuditLog { OrganizationId = organization.Id, UserId = owner.Id, Action = "seed.created", EntityName = "DevelopmentSeed" });
        await db.SaveChangesAsync(cancellationToken);
    }
}
