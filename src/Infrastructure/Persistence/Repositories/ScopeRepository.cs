using Application.Interfaces;
using Domain.AppRegistrations;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Infrastructure.Persistence.Repositories;

public class ScopeRepository : IScopeRepository
{
    private readonly AuthDbContext _context;

    public ScopeRepository(AuthDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyCollection<Scope>> GetByNamesAsync(IEnumerable<string> names, CancellationToken cancellationToken)
    {
        var normalized = names
            .Where(n => !string.IsNullOrWhiteSpace(n))
            .Select(n => n.Trim())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        if (normalized.Count == 0)
        {
            return Array.Empty<Scope>();
        }

        return await _context.Scopes
            .Where(s => normalized.Contains(s.Name) && s.IsActive)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<Scope>> GetDefaultsAsync(CancellationToken cancellationToken) =>
        await _context.Scopes
            .Where(s => s.IsDefault && s.IsActive)
            .ToListAsync(cancellationToken);
}
