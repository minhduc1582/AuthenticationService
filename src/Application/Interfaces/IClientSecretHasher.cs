using Application.AppRegistrations.Models;

namespace Application.Interfaces;

public interface IClientSecretHasher
{
    ClientSecretHashResult HashSecret(string secret);
    bool Verify(string secret, string hash, string salt);
}
