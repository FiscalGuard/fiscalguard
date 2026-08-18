using FiscalGuard.Domain;
using FiscalGuard.Infrastructure;
using FiscalGuard.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace FiscalGuard.Tests;

public sealed class PlanLimitServiceTests
{
    [Fact]
    public async Task EnsureCanAddCompanyAsync_BlocksWhenLimitIsReached()
    {
        var options = new DbContextOptionsBuilder<FiscalGuardDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        await using var db = new FiscalGuardDbContext(options);
        var organizationId = Guid.NewGuid();
        var plan = new Plan { Code = "trial-test", Name = "Trial Test", CnpjLimit = 1, UserLimit = 2, TrialDays = 14 };
        var customer = new Customer { OrganizationId = organizationId, LegalName = "Cliente" };
        db.Plans.Add(plan);
        db.Subscriptions.Add(new Subscription { OrganizationId = organizationId, Plan = plan, Status = SubscriptionStatus.Trial, TrialEndsAt = DateTimeOffset.UtcNow.AddDays(10) });
        db.Customers.Add(customer);
        db.Companies.Add(new Company { OrganizationId = organizationId, Customer = customer, LegalName = "Empresa", Cnpj = "11222333000181", TaxRegime = TaxRegime.SimplesNacional });
        await db.SaveChangesAsync();

        var service = new PlanLimitService(db);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.EnsureCanAddCompanyAsync(organizationId, CancellationToken.None));
    }
}
