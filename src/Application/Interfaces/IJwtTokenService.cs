using Application.AppRegistrations.Dtos;
using Domain.AppRegistrations;

namespace Application.Interfaces;

public interface IJwtTokenService
{
    Task<TokenResponseDto> IssueClientCredentialsTokenAsync(
        AppRegistration appRegistration,
        IReadOnlyCollection<string> scopes,
        CancellationToken cancellationToken);
}
