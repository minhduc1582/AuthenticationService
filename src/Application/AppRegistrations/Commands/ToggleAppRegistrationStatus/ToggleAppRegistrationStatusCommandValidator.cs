using FluentValidation;

namespace Application.AppRegistrations.Commands.ToggleAppRegistrationStatus;

public class ToggleAppRegistrationStatusCommandValidator : AbstractValidator<ToggleAppRegistrationStatusCommand>
{
    public ToggleAppRegistrationStatusCommandValidator()
    {
        RuleFor(c => c.AppRegistrationId).NotEmpty();
    }
}
