using FiscalGuard.Domain;

namespace FiscalGuard.Application;

public interface ICurrentUserContext
{
    Guid OrganizationId { get; }
    Guid UserId { get; }
    UserRole Role { get; }
}
