using Application.AppRegistrations.Dtos;
using Application.AppRegistrations.Mappings;
using Application.AppRegistrations.Models;
using Application.Interfaces;
using Domain.AppRegistrations;
using MediatR;

namespace Application.AppRegistrations.Commands.CreateAppRegistration;

public record CreateAppRegistrationCommand(
    string Name,
    string? Description,
    IReadOnlyCollection<string>? Scopes,
    DateTimeOffset? ExpirationUtc) : IRequest<CreateAppRegistrationResult>;

public record CreateAppRegistrationResult(
    AppRegistrationDetailsDto AppRegistration,
    ClientSecretResponseDto ClientSecret);

public class CreateAppRegistrationCommandHandler : IRequestHandler<CreateAppRegistrationCommand, CreateAppRegistrationResult>
{
    private readonly IAppRegistrationRepository _repository;
    private readonly IScopeRepository _scopeRepository;
    private readonly ICurrentUserService _currentUser;
    private readonly IClientSecretGenerator _secretGenerator;
    private readonly IClientSecretHasher _secretHasher;

    public CreateAppRegistrationCommandHandler(
        IAppRegistrationRepository repository,
        IScopeRepository scopeRepository,
        ICurrentUserService currentUser,
        IClientSecretGenerator secretGenerator,
        IClientSecretHasher secretHasher)
    {
        _repository = repository;
        _scopeRepository = scopeRepository;
        _currentUser = currentUser;
        _secretGenerator = secretGenerator;
        _secretHasher = secretHasher;
    }

    public async Task<CreateAppRegistrationResult> Handle(CreateAppRegistrationCommand request, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is null)
        {
            throw new UnauthorizedAccessException("Current user context is required.");
        }

        var scopeNames = (request.Scopes ?? Array.Empty<string>())
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Select(s => s.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        IReadOnlyCollection<Scope> resolvedScopes;

        if (scopeNames.Length == 0)
        {
            resolvedScopes = await _scopeRepository.GetDefaultsAsync(cancellationToken);
            if (resolvedScopes.Count == 0)
            {
                throw new InvalidOperationException("No default scopes are configured.");
            }
        }
        else
        {
            resolvedScopes = await _scopeRepository.GetByNamesAsync(scopeNames, cancellationToken);
            if (resolvedScopes.Count != scopeNames.Length)
            {
                throw new InvalidOperationException("One or more requested scopes are invalid.");
            }
        }

        var secret = _secretGenerator.GenerateClientSecret();
        var secretHash = _secretHasher.HashSecret(secret);

        var clientId = await GenerateUniqueClientId(cancellationToken);

        var registration = AppRegistration.Create(
            request.Name.Trim(),
            clientId,
            secretHash.Hash,
            secretHash.Salt,
            _currentUser.UserId.Value,
            resolvedScopes,
            request.Description?.Trim(),
            request.ExpirationUtc);

        await _repository.AddAsync(registration, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return new CreateAppRegistrationResult(
            AppRegistrationMapper.ToDetailsDto(registration),
            new ClientSecretResponseDto(registration.ClientId, secret, DateTimeOffset.UtcNow));
    }

    private async Task<string> GenerateUniqueClientId(CancellationToken cancellationToken)
    {
        const int maxAttempts = 10;
        for (var attempt = 0; attempt < maxAttempts; attempt++)
        {
            var clientId = _secretGenerator.GenerateClientId();
            var exists = await _repository.ClientIdExistsAsync(clientId, cancellationToken);
            if (!exists)
            {
                return clientId;
            }
        }

        throw new InvalidOperationException("Unable to generate a unique client identifier.");
    }
}
