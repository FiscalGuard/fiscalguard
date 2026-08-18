using FiscalGuard.Application;
using FiscalGuard.Domain;
using FiscalGuard.Infrastructure;
using FiscalGuard.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace FiscalGuard.Tests;

public sealed class IssueDeduplicationTests
{
    [Fact]
    public async Task ConsultCompanyAsync_UpdatesExistingIssueInsteadOfDuplicating()
    {
        var options = new DbContextOptionsBuilder<FiscalGuardDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        await using var db = new FiscalGuardDbContext(options);
        var organizationId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var customer = new Customer { OrganizationId = organizationId, LegalName = "Cliente" };
        var company = new Company { OrganizationId = organizationId, Customer = customer, LegalName = "Empresa", Cnpj = "11222333000182", TaxRegime = TaxRegime.SimplesNacional };
        db.Customers.Add(customer);
        db.Companies.Add(company);
        await db.SaveChangesAsync();

        var service = new FiscalMonitoringService(db, new TestCurrentUserContext(organizationId, userId), new StaticFiscalDataProvider());
        await service.ConsultCompanyAsync(company.Id, CancellationToken.None);
        await service.ConsultCompanyAsync(company.Id, CancellationToken.None);

        Assert.Equal(1, await db.FiscalIssues.CountAsync());
        Assert.Equal(2, await db.FiscalConsultations.CountAsync());
    }

    private sealed record TestCurrentUserContext(Guid OrganizationId, Guid UserId) : ICurrentUserContext
    {
        public UserRole Role => UserRole.Owner;
    }

    private sealed class StaticFiscalDataProvider : IFiscalDataProvider
    {
        public Task<FiscalConsultationResult> ConsultAsync(string cnpj, CancellationToken cancellationToken)
        {
            FiscalFinding[] findings = [new("same-finding", IssueType.MissingFiling, "Declaracao pendente", "Teste", IssueSeverity.High)];
            return Task.FromResult(new FiscalConsultationResult("test", FiscalStatus.Irregular, true, "{}", null, findings));
        }
    }
}
