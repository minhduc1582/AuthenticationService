using Application.Authorization;
using Application.Interfaces;
using MediatR;

namespace Application.AppRegistrations.Commands.ToggleAppRegistrationStatus;

public record ToggleAppRegistrationStatusCommand(Guid AppRegistrationId, bool IsEnabled) : IRequest<Unit>;

public class ToggleAppRegistrationStatusCommandHandler : IRequestHandler<ToggleAppRegistrationStatusCommand, Unit>
{
    private readonly IAppRegistrationRepository _repository;
    private readonly ICurrentUserService _currentUser;

    public ToggleAppRegistrationStatusCommandHandler(IAppRegistrationRepository repository, ICurrentUserService currentUser)
    {
        _repository = repository;
        _currentUser = currentUser;
    }

    public async Task<Unit> Handle(ToggleAppRegistrationStatusCommand request, CancellationToken cancellationToken)
    {
        var app = await _repository.GetByIdAsync(request.AppRegistrationId, cancellationToken)
                  ?? throw new KeyNotFoundException("App registration not found.");

        if (!_currentUser.IsInRole(RoleNames.Admin) && _currentUser.UserId != app.OwnerUserId)
        {
            throw new UnauthorizedAccessException("You cannot manage this application.");
        }

        if (request.IsEnabled)
        {
            app.Enable();
        }
        else
        {
            app.Disable();
        }

        await _repository.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
