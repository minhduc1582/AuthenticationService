using Application.AppRegistrations.Dtos;
using Application.Interfaces;
using MediatR;

namespace Application.AppRegistrations.Commands.IssueClientCredentialsToken;

public record IssueClientCredentialsTokenCommand(
    string GrantType,
    string ClientId,
    string ClientSecret,
    string? Scope) : IRequest<TokenResponseDto>;

public class IssueClientCredentialsTokenCommandHandler : IRequestHandler<IssueClientCredentialsTokenCommand, TokenResponseDto>
{
    private readonly IAppRegistrationRepository _repository;
    private readonly IClientSecretHasher _secretHasher;
    private readonly IJwtTokenService _jwtTokenService;

    public IssueClientCredentialsTokenCommandHandler(
        IAppRegistrationRepository repository,
        IClientSecretHasher secretHasher,
        IJwtTokenService jwtTokenService)
    {
        _repository = repository;
        _secretHasher = secretHasher;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<TokenResponseDto> Handle(IssueClientCredentialsTokenCommand request, CancellationToken cancellationToken)
    {
        if (!string.Equals(request.GrantType, "client_credentials", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Unsupported grant type.");
        }

        var app = await _repository.GetByClientIdAsync(request.ClientId, cancellationToken)
                  ?? throw new KeyNotFoundException("Invalid client credentials.");

        if (!_secretHasher.Verify(request.ClientSecret, app.ClientSecretHash, app.ClientSecretSalt))
        {
            throw new UnauthorizedAccessException("Invalid client credentials.");
        }

        if (!app.IsEnabled)
        {
            throw new InvalidOperationException("The application is disabled.");
        }

        if (app.IsExpired(DateTimeOffset.UtcNow))
        {
            throw new InvalidOperationException("The application registration has expired.");
        }

        var allowedScopes = app.Scopes
            .Select(s => s.Scope?.Name ?? s.ScopeName)
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        if (allowedScopes.Count == 0)
        {
            throw new InvalidOperationException("No scopes are configured for this application.");
        }

        var requestedScopes = string.IsNullOrWhiteSpace(request.Scope)
            ? allowedScopes
            : request.Scope.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

        if (!requestedScopes.IsSubsetOf(allowedScopes))
        {
            throw new InvalidOperationException("Requested scope is not allowed for this application.");
        }

        return await _jwtTokenService.IssueClientCredentialsTokenAsync(app, requestedScopes.ToArray(), cancellationToken);
    }
}
