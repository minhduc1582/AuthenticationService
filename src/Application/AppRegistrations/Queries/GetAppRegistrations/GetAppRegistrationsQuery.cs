using Application.AppRegistrations.Dtos;
using Application.AppRegistrations.Mappings;
using Application.Authorization;
using Application.Interfaces;
using MediatR;

namespace Application.AppRegistrations.Queries.GetAppRegistrations;

public record GetAppRegistrationsQuery : IRequest<IReadOnlyCollection<AppRegistrationDto>>;

public class GetAppRegistrationsQueryHandler : IRequestHandler<GetAppRegistrationsQuery, IReadOnlyCollection<AppRegistrationDto>>
{
    private readonly IAppRegistrationRepository _repository;
    private readonly ICurrentUserService _currentUser;

    public GetAppRegistrationsQueryHandler(IAppRegistrationRepository repository, ICurrentUserService currentUser)
    {
        _repository = repository;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyCollection<AppRegistrationDto>> Handle(GetAppRegistrationsQuery request, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is null && !_currentUser.IsInRole(RoleNames.Admin))
        {
            throw new UnauthorizedAccessException("Current user context is required.");
        }

        Guid? ownerId = _currentUser.IsInRole(RoleNames.Admin) ? null : _currentUser.UserId;
        var apps = await _repository.ListAsync(ownerId, cancellationToken);
        return apps.Select(AppRegistrationMapper.ToDto).ToArray();
    }
}
