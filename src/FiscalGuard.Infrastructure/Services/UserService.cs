using FiscalGuard.Application;
using FiscalGuard.Domain;
using Microsoft.EntityFrameworkCore;

namespace FiscalGuard.Infrastructure.Services;

public sealed class UserService(
    FiscalGuardDbContext db,
    ICurrentUserContext currentUser,
    TokenFactory tokenFactory) : IUserService
{
    public async Task<IReadOnlyList<OrganizationUserDto>> ListAsync(CancellationToken cancellationToken) =>
        await db.OrganizationUsers
            .Include(x => x.User)
            .Where(x => x.OrganizationId == currentUser.OrganizationId)
            .OrderByDescending(x => x.Role == UserRole.Owner)
            .ThenBy(x => x.User!.Name)
            .Select(x => new OrganizationUserDto(
                x.Id,
                x.UserId,
                x.User!.Name,
                x.User.Email,
                x.User.Phone,
                x.Role,
                x.IsActive && x.User.IsActive,
                x.CreatedAt))
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<UserInvitationDto>> ListInvitationsAsync(CancellationToken cancellationToken) =>
        await db.UserInvitations
            .Where(x => x.OrganizationId == currentUser.OrganizationId)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new UserInvitationDto(x.Id, x.Email, x.Role, x.ExpiresAt, x.AcceptedAt))
            .ToListAsync(cancellationToken);

    public async Task<UserInvitationDto> InviteAsync(InviteUserRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
        {
            throw new InvalidOperationException("E-mail do convite e obrigatorio.");
        }

        await EnsureCanAddUserAsync(cancellationToken);
        var email = request.Email.Trim().ToLowerInvariant();
        var alreadyMember = await db.OrganizationUsers
            .Include(x => x.User)
            .AnyAsync(x => x.OrganizationId == currentUser.OrganizationId && x.User!.Email == email, cancellationToken);

        if (alreadyMember)
        {
            throw new InvalidOperationException("Usuario ja pertence a organizacao.");
        }

        var openInvitation = await db.UserInvitations.SingleOrDefaultAsync(
            x => x.OrganizationId == currentUser.OrganizationId && x.Email == email && x.AcceptedAt == null && x.ExpiresAt > DateTimeOffset.UtcNow,
            cancellationToken);

        if (openInvitation is not null)
        {
            return new UserInvitationDto(openInvitation.Id, openInvitation.Email, openInvitation.Role, openInvitation.ExpiresAt, openInvitation.AcceptedAt);
        }

        var invitationToken = tokenFactory.CreateRefreshToken();
        var invitation = new UserInvitation
        {
            OrganizationId = currentUser.OrganizationId,
            Email = email,
            Role = request.Role,
            TokenHash = tokenFactory.HashToken(invitationToken),
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(7)
        };

        db.UserInvitations.Add(invitation);
        db.AuditLogs.Add(new AuditLog
        {
            OrganizationId = currentUser.OrganizationId,
            UserId = currentUser.UserId,
            Action = "user.invited",
            EntityName = nameof(UserInvitation),
            EntityId = invitation.Id.ToString(),
            AfterValues = email
        });

        await db.SaveChangesAsync(cancellationToken);
        return new UserInvitationDto(invitation.Id, invitation.Email, invitation.Role, invitation.ExpiresAt, invitation.AcceptedAt);
    }

    public async Task<OrganizationUserDto> UpdateRoleAsync(Guid userId, UpdateUserRoleRequest request, CancellationToken cancellationToken)
    {
        var membership = await GetMembershipAsync(userId, cancellationToken);
        if (membership.Role == UserRole.Owner && request.Role != UserRole.Owner)
        {
            await EnsureAnotherActiveOwnerAsync(userId, cancellationToken);
        }

        membership.Role = request.Role;
        db.AuditLogs.Add(new AuditLog
        {
            OrganizationId = currentUser.OrganizationId,
            UserId = currentUser.UserId,
            Action = "user.role.updated",
            EntityName = nameof(OrganizationUser),
            EntityId = membership.Id.ToString(),
            AfterValues = request.Role.ToString()
        });

        await db.SaveChangesAsync(cancellationToken);
        return ToDto(membership);
    }

    public async Task<OrganizationUserDto> UpdateStatusAsync(Guid userId, UpdateUserStatusRequest request, CancellationToken cancellationToken)
    {
        var membership = await GetMembershipAsync(userId, cancellationToken);
        if (!request.IsActive && membership.Role == UserRole.Owner)
        {
            await EnsureAnotherActiveOwnerAsync(userId, cancellationToken);
        }

        membership.IsActive = request.IsActive;
        if (membership.User is not null)
        {
            membership.User.IsActive = request.IsActive;
        }

        db.AuditLogs.Add(new AuditLog
        {
            OrganizationId = currentUser.OrganizationId,
            UserId = currentUser.UserId,
            Action = "user.status.updated",
            EntityName = nameof(OrganizationUser),
            EntityId = membership.Id.ToString(),
            AfterValues = request.IsActive.ToString()
        });

        await db.SaveChangesAsync(cancellationToken);
        return ToDto(membership);
    }

    private async Task EnsureCanAddUserAsync(CancellationToken cancellationToken)
    {
        var subscription = await db.Subscriptions
            .Include(x => x.Plan)
            .SingleAsync(x => x.OrganizationId == currentUser.OrganizationId, cancellationToken);

        var activeUsers = await db.OrganizationUsers.CountAsync(
            x => x.OrganizationId == currentUser.OrganizationId && x.IsActive,
            cancellationToken);

        var openInvitations = await db.UserInvitations.CountAsync(
            x => x.OrganizationId == currentUser.OrganizationId && x.AcceptedAt == null && x.ExpiresAt > DateTimeOffset.UtcNow,
            cancellationToken);

        if (activeUsers + openInvitations >= subscription.Plan!.UserLimit)
        {
            throw new InvalidOperationException("Limite de usuarios do plano atingido.");
        }
    }

    private async Task<OrganizationUser> GetMembershipAsync(Guid userId, CancellationToken cancellationToken) =>
        await db.OrganizationUsers
            .Include(x => x.User)
            .SingleAsync(x => x.OrganizationId == currentUser.OrganizationId && x.UserId == userId, cancellationToken);

    private async Task EnsureAnotherActiveOwnerAsync(Guid userId, CancellationToken cancellationToken)
    {
        var hasAnotherOwner = await db.OrganizationUsers.AnyAsync(
            x => x.OrganizationId == currentUser.OrganizationId
                && x.UserId != userId
                && x.Role == UserRole.Owner
                && x.IsActive,
            cancellationToken);

        if (!hasAnotherOwner)
        {
            throw new InvalidOperationException("Nao e permitido remover ou inativar o ultimo proprietario da organizacao.");
        }
    }

    private static OrganizationUserDto ToDto(OrganizationUser membership) =>
        new(
            membership.Id,
            membership.UserId,
            membership.User!.Name,
            membership.User.Email,
            membership.User.Phone,
            membership.Role,
            membership.IsActive && membership.User.IsActive,
            membership.CreatedAt);
}
