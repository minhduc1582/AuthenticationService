using System.Security.Cryptography;
using Application.Interfaces;

namespace Infrastructure.Auth.Secrets;

public class ClientSecretGenerator : IClientSecretGenerator
{
    public string GenerateClientId()
    {
        Span<byte> buffer = stackalloc byte[16];
        RandomNumberGenerator.Fill(buffer);
        return Convert.ToHexString(buffer).ToLowerInvariant();
    }

    public string GenerateClientSecret(int sizeInBytes = 32)
    {
        var bytes = RandomNumberGenerator.GetBytes(sizeInBytes);
        return Convert.ToBase64String(bytes);
    }
}
