using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Application.AppRegistrations.Dtos;
using Application.Interfaces;
using Application.Options;
using Domain.AppRegistrations;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Auth.Tokens;

public class JwtTokenService : IJwtTokenService
{
    private readonly JwtOptions _options;

    public JwtTokenService(IOptions<JwtOptions> options)
    {
        _options = options.Value;
    }

    public Task<TokenResponseDto> IssueClientCredentialsTokenAsync(
        AppRegistration appRegistration,
        IReadOnlyCollection<string> scopes,
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var expires = now.AddMinutes(_options.AccessTokenLifetimeMinutes);

        var claims = new List<Claim>
        {
            new("client_id", appRegistration.ClientId),
            new("sub", appRegistration.Id.ToString()),
            new("owner_id", appRegistration.OwnerUserId.ToString())
        };

        foreach (var scope in scopes)
        {
            claims.Add(new("scope", scope));
        }

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SigningKey));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var jwt = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            notBefore: now,
            expires: expires,
            signingCredentials: credentials);

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.WriteToken(jwt);

        var response = new TokenResponseDto(
            token,
            "Bearer",
            (int)(expires - now).TotalSeconds,
            string.Join(' ', scopes));

        return Task.FromResult(response);
    }
}
