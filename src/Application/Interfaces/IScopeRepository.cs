using Domain.AppRegistrations;

namespace Application.Interfaces;

public interface IScopeRepository
{
    Task<IReadOnlyCollection<Scope>> GetByNamesAsync(IEnumerable<string> names, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<Scope>> GetDefaultsAsync(CancellationToken cancellationToken);
}
