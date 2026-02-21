namespace Application.Interfaces;

public interface IClientSecretGenerator
{
    string GenerateClientId();
    string GenerateClientSecret(int sizeInBytes = 32);
}
