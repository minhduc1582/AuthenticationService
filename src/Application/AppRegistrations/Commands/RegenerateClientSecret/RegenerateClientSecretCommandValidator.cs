using FluentValidation;

namespace Application.AppRegistrations.Commands.RegenerateClientSecret;

public class RegenerateClientSecretCommandValidator : AbstractValidator<RegenerateClientSecretCommand>
{
    public RegenerateClientSecretCommandValidator()
    {
        RuleFor(c => c.AppRegistrationId)
            .NotEmpty();
    }
}
