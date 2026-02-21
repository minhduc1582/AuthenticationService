using System.Security.Cryptography;
using Application.AppRegistrations.Models;
using Application.Interfaces;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace Infrastructure.Auth.Secrets;

public class ClientSecretHasher : IClientSecretHasher
{
    private const int Iterations = 100_000;

    public ClientSecretHashResult HashSecret(string secret)
    {
        var salt = RandomNumberGenerator.GetBytes(32);
        var hash = Convert.ToBase64String(KeyDerivation.Pbkdf2(
            secret,
            salt,
            KeyDerivationPrf.HMACSHA512,
            Iterations,
            64));

        return new ClientSecretHashResult(hash, Convert.ToBase64String(salt));
    }

    public bool Verify(string secret, string hash, string salt)
    {
        var saltBytes = Convert.FromBase64String(salt);
        var computedHash = Convert.ToBase64String(KeyDerivation.Pbkdf2(
            secret,
            saltBytes,
            KeyDerivationPrf.HMACSHA512,
            Iterations,
            64));

        var hashBytes = Convert.FromBase64String(hash);
        var computedBytes = Convert.FromBase64String(computedHash);

        if (hashBytes.Length != computedBytes.Length)
        {
            return false;
        }

        return CryptographicOperations.FixedTimeEquals(hashBytes, computedBytes);
    }
}
