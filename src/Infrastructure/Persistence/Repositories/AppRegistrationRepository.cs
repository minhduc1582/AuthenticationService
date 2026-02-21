using Application.Interfaces;
using Domain.AppRegistrations;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Infrastructure.Persistence.Repositories;

public class AppRegistrationRepository : IAppRegistrationRepository
{
    private readonly AuthDbContext _context;

    public AppRegistrationRepository(AuthDbContext context)
    {
        _context = context;
    }

    public async Task<bool> ClientIdExistsAsync(string clientId, CancellationToken cancellationToken) =>
        await _context.AppRegistrations.AnyAsync(a => a.ClientId == clientId, cancellationToken);

    public async Task AddAsync(AppRegistration appRegistration, CancellationToken cancellationToken) =>
        await _context.AppRegistrations.AddAsync(appRegistration, cancellationToken);

    public Task<AppRegistration?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        _context.AppRegistrations
            .Include(a => a.Scopes)
            .ThenInclude(s => s.Scope)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

    public Task<AppRegistration?> GetByClientIdAsync(string clientId, CancellationToken cancellationToken) =>
        _context.AppRegistrations
            .Include(a => a.Scopes)
            .ThenInclude(s => s.Scope)
            .FirstOrDefaultAsync(a => a.ClientId == clientId, cancellationToken);

    public async Task<IReadOnlyCollection<AppRegistration>> ListAsync(Guid? ownerUserId, CancellationToken cancellationToken)
    {
        var query = _context.AppRegistrations
            .Include(a => a.Scopes)
            .ThenInclude(s => s.Scope)
            .AsQueryable();

        if (ownerUserId.HasValue)
        {
            query = query.Where(a => a.OwnerUserId == ownerUserId.Value);
        }

        return await query.OrderByDescending(a => a.CreatedAt).ToListAsync(cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        _context.SaveChangesAsync(cancellationToken);
}
