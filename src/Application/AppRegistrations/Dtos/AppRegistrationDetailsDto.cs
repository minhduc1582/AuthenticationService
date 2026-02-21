namespace Application.AppRegistrations.Dtos;

public record AppRegistrationDetailsDto(
    Guid Id,
    string Name,
    string? Description,
    string ClientId,
    Guid OwnerUserId,
    bool IsEnabled,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt,
    DateTimeOffset? ExpirationUtc,
    DateTimeOffset? LastSecretRotatedAt,
    IReadOnlyCollection<string> Scopes);
