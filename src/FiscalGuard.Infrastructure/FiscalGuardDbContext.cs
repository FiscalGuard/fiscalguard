using FiscalGuard.Domain;
using Microsoft.EntityFrameworkCore;

namespace FiscalGuard.Infrastructure;

public sealed class FiscalGuardDbContext(DbContextOptions<FiscalGuardDbContext> options) : DbContext(options)
{
    public DbSet<Organization> Organizations => Set<Organization>();
    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<OrganizationUser> OrganizationUsers => Set<OrganizationUser>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Company> Companies => Set<Company>();
    public DbSet<FiscalConsultation> FiscalConsultations => Set<FiscalConsultation>();
    public DbSet<FiscalConsultationItem> FiscalConsultationItems => Set<FiscalConsultationItem>();
    public DbSet<FiscalStatusHistory> FiscalStatusHistory => Set<FiscalStatusHistory>();
    public DbSet<FiscalIssue> FiscalIssues => Set<FiscalIssue>();
    public DbSet<FiscalIssueHistory> FiscalIssueHistory => Set<FiscalIssueHistory>();
    public DbSet<Alert> Alerts => Set<Alert>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<Plan> Plans => Set<Plan>();
    public DbSet<Subscription> Subscriptions => Set<Subscription>();
    public DbSet<SubscriptionUsage> SubscriptionUsage => Set<SubscriptionUsage>();
    public DbSet<JobExecution> JobExecutions => Set<JobExecution>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<UserInvitation> UserInvitations => Set<UserInvitation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Organization>().Property(x => x.Name).HasMaxLength(180);
        modelBuilder.Entity<AppUser>().HasIndex(x => x.Email).IsUnique();
        modelBuilder.Entity<AppUser>().Property(x => x.Email).HasMaxLength(180);
        modelBuilder.Entity<OrganizationUser>().HasIndex(x => new { x.OrganizationId, x.UserId }).IsUnique();

        modelBuilder.Entity<Customer>().HasIndex(x => x.OrganizationId);
        modelBuilder.Entity<Company>().HasIndex(x => x.OrganizationId);
        modelBuilder.Entity<Company>().HasIndex(x => new { x.OrganizationId, x.Cnpj }).IsUnique();
        modelBuilder.Entity<Company>().HasIndex(x => new { x.OrganizationId, x.FiscalStatus });
        modelBuilder.Entity<Company>().HasIndex(x => new { x.OrganizationId, x.NextConsultationAt });
        modelBuilder.Entity<Company>().Property(x => x.Cnpj).HasMaxLength(14);

        modelBuilder.Entity<FiscalConsultation>().HasIndex(x => new { x.OrganizationId, x.CompanyId, x.CreatedAt });
        modelBuilder.Entity<FiscalIssue>().HasIndex(x => new { x.OrganizationId, x.Status });
        modelBuilder.Entity<FiscalIssue>().HasIndex(x => new { x.OrganizationId, x.DeduplicationKey }).IsUnique();
        modelBuilder.Entity<Alert>().HasIndex(x => new { x.OrganizationId, x.IsRead, x.CreatedAt });
        modelBuilder.Entity<Notification>().HasIndex(x => new { x.OrganizationId, x.Status, x.CreatedAt });
        modelBuilder.Entity<Plan>().HasIndex(x => x.Code).IsUnique();
        modelBuilder.Entity<Subscription>().HasIndex(x => x.OrganizationId);
        modelBuilder.Entity<RefreshToken>().HasIndex(x => x.TokenHash).IsUnique();

        SeedPlans(modelBuilder);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        foreach (var entry in ChangeTracker.Entries<Entity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = now;
            }

            if (entry.State is EntityState.Added or EntityState.Modified)
            {
                entry.Entity.UpdatedAt = now;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }

    private static void SeedPlans(ModelBuilder modelBuilder)
    {
        var trialCreatedAt = new DateTimeOffset(new DateTime(2026, 8, 4, 23, 57, 48, 760, DateTimeKind.Unspecified).AddTicks(7361), TimeSpan.Zero);
        var trialUpdatedAt = new DateTimeOffset(new DateTime(2026, 8, 4, 23, 57, 48, 760, DateTimeKind.Unspecified).AddTicks(7368), TimeSpan.Zero);
        var freeCreatedAt = new DateTimeOffset(new DateTime(2026, 8, 4, 23, 57, 48, 760, DateTimeKind.Unspecified).AddTicks(7408), TimeSpan.Zero);
        var freeUpdatedAt = new DateTimeOffset(new DateTime(2026, 8, 4, 23, 57, 48, 760, DateTimeKind.Unspecified).AddTicks(7409), TimeSpan.Zero);
        var professionalCreatedAt = new DateTimeOffset(new DateTime(2026, 8, 4, 23, 57, 48, 760, DateTimeKind.Unspecified).AddTicks(7416), TimeSpan.Zero);
        var professionalUpdatedAt = new DateTimeOffset(new DateTime(2026, 8, 4, 23, 57, 48, 760, DateTimeKind.Unspecified).AddTicks(7417), TimeSpan.Zero);
        var officeCreatedAt = new DateTimeOffset(new DateTime(2026, 8, 4, 23, 57, 48, 760, DateTimeKind.Unspecified).AddTicks(7422), TimeSpan.Zero);
        var officeUpdatedAt = new DateTimeOffset(new DateTime(2026, 8, 4, 23, 57, 48, 760, DateTimeKind.Unspecified).AddTicks(7423), TimeSpan.Zero);
        modelBuilder.Entity<Plan>().HasData(
            new Plan { Id = Guid.Parse("10000000-0000-0000-0000-000000000001"), Code = "trial", Name = "Trial", CnpjLimit = 5, UserLimit = 3, TrialDays = 14, AutomatedMonitoring = false, EmailAlerts = false, FullHistory = false, CreatedAt = trialCreatedAt, UpdatedAt = trialUpdatedAt },
            new Plan { Id = Guid.Parse("10000000-0000-0000-0000-000000000002"), Code = "free", Name = "Gratuito", CnpjLimit = 2, UserLimit = 1, TrialDays = 0, AutomatedMonitoring = false, EmailAlerts = false, FullHistory = false, CreatedAt = freeCreatedAt, UpdatedAt = freeUpdatedAt },
            new Plan { Id = Guid.Parse("10000000-0000-0000-0000-000000000003"), Code = "professional", Name = "Profissional", CnpjLimit = 50, UserLimit = 8, TrialDays = 0, AutomatedMonitoring = true, EmailAlerts = true, FullHistory = true, CreatedAt = professionalCreatedAt, UpdatedAt = professionalUpdatedAt },
            new Plan { Id = Guid.Parse("10000000-0000-0000-0000-000000000004"), Code = "office", Name = "Escritorio", CnpjLimit = 250, UserLimit = 30, TrialDays = 0, AutomatedMonitoring = true, EmailAlerts = true, FullHistory = true, CreatedAt = officeCreatedAt, UpdatedAt = officeUpdatedAt });
    }
}
