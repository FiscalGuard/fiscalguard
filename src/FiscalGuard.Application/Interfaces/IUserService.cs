namespace FiscalGuard.Application;

public interface IUserService
{
    Task<IReadOnlyList<OrganizationUserDto>> ListAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<UserInvitationDto>> ListInvitationsAsync(CancellationToken cancellationToken);
    Task<UserInvitationDto> InviteAsync(InviteUserRequest request, CancellationToken cancellationToken);
    Task<OrganizationUserDto> UpdateRoleAsync(Guid userId, UpdateUserRoleRequest request, CancellationToken cancellationToken);
    Task<OrganizationUserDto> UpdateStatusAsync(Guid userId, UpdateUserStatusRequest request, CancellationToken cancellationToken);
}
