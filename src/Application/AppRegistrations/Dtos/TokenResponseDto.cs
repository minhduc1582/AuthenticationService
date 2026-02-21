namespace Application.AppRegistrations.Dtos;

public record TokenResponseDto(
    string AccessToken,
    string TokenType,
    int ExpiresIn,
    string Scope);
