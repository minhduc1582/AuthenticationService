using Application.AppRegistrations.Dtos;
using Application.Authorization;
using Application.Interfaces;
using MediatR;

namespace Application.AppRegistrations.Commands.RegenerateClientSecret;

public record RegenerateClientSecretCommand(Guid AppRegistrationId) : IRequest<ClientSecretResponseDto>;

public class RegenerateClientSecretCommandHandler : IRequestHandler<RegenerateClientSecretCommand, ClientSecretResponseDto>
{
    private readonly IAppRegistrationRepository _repository;
    private readonly IClientSecretGenerator _secretGenerator;
    private readonly IClientSecretHasher _secretHasher;
    private readonly ICurrentUserService _currentUser;

    public RegenerateClientSecretCommandHandler(
        IAppRegistrationRepository repository,
        IClientSecretGenerator secretGenerator,
        IClientSecretHasher secretHasher,
        ICurrentUserService currentUser)
    {
        _repository = repository;
        _secretGenerator = secretGenerator;
        _secretHasher = secretHasher;
        _currentUser = currentUser;
    }

    public async Task<ClientSecretResponseDto> Handle(RegenerateClientSecretCommand request, CancellationToken cancellationToken)
    {
        var app = await _repository.GetByIdAsync(request.AppRegistrationId, cancellationToken)
                  ?? throw new KeyNotFoundException("App registration not found.");

        if (!_currentUser.IsInRole(RoleNames.Admin) && _currentUser.UserId != app.OwnerUserId)
        {
            throw new UnauthorizedAccessException("You cannot manage this application.");
        }

        var newSecret = _secretGenerator.GenerateClientSecret();
        var hash = _secretHasher.HashSecret(newSecret);
        app.RotateSecret(hash.Hash, hash.Salt);

        await _repository.SaveChangesAsync(cancellationToken);

        return new ClientSecretResponseDto(app.ClientId, newSecret, DateTimeOffset.UtcNow);
    }
}
