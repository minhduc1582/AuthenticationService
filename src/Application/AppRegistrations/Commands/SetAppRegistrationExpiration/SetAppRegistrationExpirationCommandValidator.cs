using FluentValidation;

namespace Application.AppRegistrations.Commands.SetAppRegistrationExpiration;

public class SetAppRegistrationExpirationCommandValidator : AbstractValidator<SetAppRegistrationExpirationCommand>
{
    public SetAppRegistrationExpirationCommandValidator()
    {
        RuleFor(c => c.AppRegistrationId).NotEmpty();
    }
}
