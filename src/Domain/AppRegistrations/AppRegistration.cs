using System.Collections.Generic;
using System.Linq;

namespace Domain.AppRegistrations;

public class AppRegistration
{
    private readonly List<AppRegistrationScope> _scopes = new();

    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string ClientId { get; private set; } = string.Empty;
    public string ClientSecretHash { get; private set; } = string.Empty;
    public string ClientSecretSalt { get; private set; } = string.Empty;
    public Guid OwnerUserId { get; private set; }
    public bool IsEnabled { get; private set; } = true;
    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; private set; }
    public DateTimeOffset? ExpirationUtc { get; private set; }
    public DateTimeOffset? LastSecretRotatedAt { get; private set; }
    public IReadOnlyCollection<AppRegistrationScope> Scopes => _scopes.AsReadOnly();

    private AppRegistration()
    {
    }

    public static AppRegistration Create(
        string name,
        string clientId,
        string clientSecretHash,
        string clientSecretSalt,
        Guid ownerUserId,
        IEnumerable<Scope> scopes,
        string? description = null,
        DateTimeOffset? expirationUtc = null)
    {
        var registration = new AppRegistration
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description,
            ClientId = clientId,
            ClientSecretHash = clientSecretHash,
            ClientSecretSalt = clientSecretSalt,
            OwnerUserId = ownerUserId,
            ExpirationUtc = expirationUtc,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        registration.UpdateScopes(scopes);
        return registration;
    }

    public void Rename(string name, string? description)
    {
        Name = name;
        Description = description;
        Touch();
    }

    public void SetExpiration(DateTimeOffset? expirationUtc)
    {
        ExpirationUtc = expirationUtc;
        Touch();
    }

    public void Enable()
    {
        IsEnabled = true;
        Touch();
    }

    public void Disable()
    {
        IsEnabled = false;
        Touch();
    }

    public void UpdateScopes(IEnumerable<Scope> scopes)
    {
        _scopes.Clear();
        foreach (var scope in scopes.DistinctBy(s => s.Id))
        {
            _scopes.Add(new AppRegistrationScope(Id, scope.Id, scope.Name));
        }

        Touch();
    }

    public void RotateSecret(string newHash, string newSalt)
    {
        ClientSecretHash = newHash;
        ClientSecretSalt = newSalt;
        LastSecretRotatedAt = DateTimeOffset.UtcNow;
        Touch();
    }

    public bool IsExpired(DateTimeOffset utcNow) =>
        ExpirationUtc.HasValue && ExpirationUtc.Value <= utcNow;

    private void Touch()
    {
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
