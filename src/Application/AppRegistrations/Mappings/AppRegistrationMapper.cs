using Application.AppRegistrations.Dtos;
using Domain.AppRegistrations;
using System.Linq;

namespace Application.AppRegistrations.Mappings;

public static class AppRegistrationMapper
{
    public static AppRegistrationDto ToDto(AppRegistration app) =>
        new(
            app.Id,
            app.Name,
            app.ClientId,
            app.IsEnabled,
            app.CreatedAt,
            app.ExpirationUtc,
            app.Scopes.Select(s => s.Scope?.Name ?? s.ScopeName).Where(name => !string.IsNullOrWhiteSpace(name)).Distinct().ToArray());

    public static AppRegistrationDetailsDto ToDetailsDto(AppRegistration app) =>
        new(
            app.Id,
            app.Name,
            app.Description,
            app.ClientId,
            app.OwnerUserId,
            app.IsEnabled,
            app.CreatedAt,
            app.UpdatedAt,
            app.ExpirationUtc,
            app.LastSecretRotatedAt,
            app.Scopes.Select(s => s.Scope?.Name ?? s.ScopeName).Where(name => !string.IsNullOrWhiteSpace(name)).Distinct().ToArray());
}
