using Domain.AppRegistrations;

namespace Application.Interfaces;

public interface IAppRegistrationRepository
{
    Task<bool> ClientIdExistsAsync(string clientId, CancellationToken cancellationToken);
    Task AddAsync(AppRegistration appRegistration, CancellationToken cancellationToken);
    Task<AppRegistration?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<AppRegistration?> GetByClientIdAsync(string clientId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<AppRegistration>> ListAsync(Guid? ownerUserId, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
