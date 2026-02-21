using Application.Authorization;
using Application.Interfaces;
using MediatR;

namespace Application.AppRegistrations.Commands.SetAppRegistrationExpiration;

public record SetAppRegistrationExpirationCommand(Guid AppRegistrationId, DateTimeOffset? ExpirationUtc) : IRequest<Unit>;

public class SetAppRegistrationExpirationCommandHandler : IRequestHandler<SetAppRegistrationExpirationCommand, Unit>
{
    private readonly IAppRegistrationRepository _repository;
    private readonly ICurrentUserService _currentUser;

    public SetAppRegistrationExpirationCommandHandler(IAppRegistrationRepository repository, ICurrentUserService currentUser)
    {
        _repository = repository;
        _currentUser = currentUser;
    }

    public async Task<Unit> Handle(SetAppRegistrationExpirationCommand request, CancellationToken cancellationToken)
    {
        var app = await _repository.GetByIdAsync(request.AppRegistrationId, cancellationToken)
                  ?? throw new KeyNotFoundException("App registration not found.");

        if (!_currentUser.IsInRole(RoleNames.Admin) && _currentUser.UserId != app.OwnerUserId)
        {
            throw new UnauthorizedAccessException("You cannot manage this application.");
        }

        if (request.ExpirationUtc.HasValue && request.ExpirationUtc <= DateTimeOffset.UtcNow)
        {
            throw new InvalidOperationException("Expiration must be a future date.");
        }

        app.SetExpiration(request.ExpirationUtc);
        await _repository.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
