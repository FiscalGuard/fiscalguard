using FiscalGuard.Application;
using FiscalGuard.Domain;
using Microsoft.EntityFrameworkCore;

namespace FiscalGuard.Infrastructure.Services;

public sealed class AuthService(
    FiscalGuardDbContext db,
    PasswordHasher hasher,
    TokenFactory tokenFactory) : IAuthService
{
    public async Task<AuthResponse> RegisterAsync(RegisterOrganizationRequest request, CancellationToken cancellationToken)
    {
        if (!request.AcceptedTerms)
        {
            throw new InvalidOperationException("Aceite dos termos e obrigatorio.");
        }

        if (request.Password.Length < 8)
        {
            throw new InvalidOperationException("Senha deve conter ao menos 8 caracteres.");
        }

        var email = request.Email.ToLowerInvariant();
        if (await db.Users.AnyAsync(x => x.Email == email, cancellationToken))
        {
            throw new InvalidOperationException("E-mail ja cadastrado.");
        }

        var trialPlan = await db.Plans.SingleAsync(x => x.Code == "trial", cancellationToken);
        var organization = new Organization
        {
            Name = request.OrganizationName,
            Document = request.OfficeDocument,
            Phone = request.Phone
        };
        var user = new AppUser
        {
            Name = request.ResponsibleName,
            Email = email,
            Phone = request.Phone,
            PasswordHash = hasher.Hash(request.Password)
        };

        db.Organizations.Add(organization);
        db.Users.Add(user);
        db.OrganizationUsers.Add(new OrganizationUser { Organization = organization, User = user, Role = UserRole.Owner });
        db.Subscriptions.Add(new Subscription
        {
            OrganizationId = organization.Id,
            Plan = trialPlan,
            Status = SubscriptionStatus.Trial,
            TrialEndsAt = DateTimeOffset.UtcNow.AddDays(trialPlan.TrialDays)
        });
        db.SubscriptionUsage.Add(new SubscriptionUsage { OrganizationId = organization.Id, ActiveUsersCount = 1 });
        db.AuditLogs.Add(new AuditLog
        {
            OrganizationId = organization.Id,
            Action = "organization.registered",
            EntityName = nameof(Organization),
            EntityId = organization.Id.ToString()
        });

        await db.SaveChangesAsync(cancellationToken);
        return await IssueTokensAsync(user, organization.Id, UserRole.Owner, cancellationToken);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        var email = request.Email.ToLowerInvariant();
        var user = await db.Users.SingleOrDefaultAsync(x => x.Email == email && x.IsActive, cancellationToken)
            ?? throw new UnauthorizedAccessException("Credenciais invalidas.");

        if (!hasher.Verify(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Credenciais invalidas.");
        }

        var membership = await db.OrganizationUsers.FirstAsync(x => x.UserId == user.Id && x.IsActive, cancellationToken);
        return await IssueTokensAsync(user, membership.OrganizationId, membership.Role, cancellationToken);
    }

    public async Task<AuthResponse> RefreshAsync(RefreshRequest request, CancellationToken cancellationToken)
    {
        var hash = tokenFactory.HashToken(request.RefreshToken);
        var refreshToken = await db.RefreshTokens
            .SingleOrDefaultAsync(x => x.TokenHash == hash && x.RevokedAt == null && x.ExpiresAt > DateTimeOffset.UtcNow, cancellationToken)
            ?? throw new UnauthorizedAccessException("Refresh token invalido.");

        refreshToken.RevokedAt = DateTimeOffset.UtcNow;
        var user = await db.Users.FindAsync([refreshToken.UserId], cancellationToken)
            ?? throw new UnauthorizedAccessException();
        var membership = await db.OrganizationUsers.FirstAsync(
            x => x.UserId == user.Id && x.OrganizationId == refreshToken.OrganizationId,
            cancellationToken);

        return await IssueTokensAsync(user, refreshToken.OrganizationId, membership.Role, cancellationToken);
    }

    private async Task<AuthResponse> IssueTokensAsync(
        AppUser user,
        Guid organizationId,
        UserRole role,
        CancellationToken cancellationToken)
    {
        var access = tokenFactory.CreateAccessToken(user, organizationId, role);
        var refresh = tokenFactory.CreateRefreshToken();

        db.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.Id,
            OrganizationId = organizationId,
            TokenHash = tokenFactory.HashToken(refresh),
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(14)
        });

        await db.SaveChangesAsync(cancellationToken);
        return new AuthResponse(access.Token, refresh, access.ExpiresAt, organizationId, user.Name, role.ToString());
    }
}
