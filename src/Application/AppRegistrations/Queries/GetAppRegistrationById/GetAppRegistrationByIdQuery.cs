using Application.AppRegistrations.Dtos;
using Application.AppRegistrations.Mappings;
using Application.Authorization;
using Application.Interfaces;
using MediatR;

namespace Application.AppRegistrations.Queries.GetAppRegistrationById;

public record GetAppRegistrationByIdQuery(Guid Id) : IRequest<AppRegistrationDetailsDto>;

public class GetAppRegistrationByIdQueryHandler : IRequestHandler<GetAppRegistrationByIdQuery, AppRegistrationDetailsDto>
{
    private readonly IAppRegistrationRepository _repository;
    private readonly ICurrentUserService _currentUser;

    public GetAppRegistrationByIdQueryHandler(IAppRegistrationRepository repository, ICurrentUserService currentUser)
    {
        _repository = repository;
        _currentUser = currentUser;
    }

    public async Task<AppRegistrationDetailsDto> Handle(GetAppRegistrationByIdQuery request, CancellationToken cancellationToken)
    {
        var app = await _repository.GetByIdAsync(request.Id, cancellationToken)
                  ?? throw new KeyNotFoundException("App registration not found.");

        if (!_currentUser.IsInRole(RoleNames.Admin) && _currentUser.UserId != app.OwnerUserId)
        {
            throw new UnauthorizedAccessException("You are not allowed to access this application.");
        }

        return AppRegistrationMapper.ToDetailsDto(app);
    }
}
