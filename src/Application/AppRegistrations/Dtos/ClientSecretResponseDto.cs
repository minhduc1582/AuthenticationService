namespace Application.AppRegistrations.Dtos;

public record ClientSecretResponseDto(
    string ClientId,
    string ClientSecret,
    DateTimeOffset GeneratedAt);
