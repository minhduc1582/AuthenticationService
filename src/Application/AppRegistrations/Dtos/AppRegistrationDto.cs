namespace Application.AppRegistrations.Dtos;

public record AppRegistrationDto(
    Guid Id,
    string Name,
    string ClientId,
    bool IsEnabled,
    DateTimeOffset CreatedAt,
    DateTimeOffset? ExpirationUtc,
    IReadOnlyCollection<string> Scopes);
