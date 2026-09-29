using FiscalGuard.Application;
using FiscalGuard.Domain;
using FiscalGuard.Infrastructure;
using FiscalGuard.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace FiscalGuard.Tests;

public sealed class UserServiceTests
{
    [Fact]
    public async Task UpdateStatusAsync_BlocksDeactivationOfLastOwner()
    {
        var options = new DbContextOptionsBuilder<FiscalGuardDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        await using var db = new FiscalGuardDbContext(options);
        var organizationId = Guid.NewGuid();
        var owner = new AppUser
        {
            Name = "Owner",
            Email = "owner@test.local",
            PasswordHash = "hash"
        };

        db.Organizations.Add(new Organization { Id = organizationId, Name = "Org" });
        db.Users.Add(owner);
        db.OrganizationUsers.Add(new OrganizationUser
        {
            OrganizationId = organizationId,
            User = owner,
            Role = UserRole.Owner,
            IsActive = true
        });
        db.Plans.Add(new Plan { Id = Guid.NewGuid(), Code = "trial-test", Name = "Trial", CnpjLimit = 5, UserLimit = 3, TrialDays = 14 });
        await db.SaveChangesAsync();

        var service = new UserService(db, new TestCurrentUserContext(organizationId, owner.Id), tokenFactory: null!);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.UpdateStatusAsync(owner.Id, new UpdateUserStatusRequest(false), CancellationToken.None));
    }

    private sealed record TestCurrentUserContext(Guid OrganizationId, Guid UserId) : ICurrentUserContext
    {
        public UserRole Role => UserRole.Owner;
    }
}
