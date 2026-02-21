using FluentValidation;

namespace Application.AppRegistrations.Commands.CreateAppRegistration;

public class CreateAppRegistrationCommandValidator : AbstractValidator<CreateAppRegistrationCommand>
{
    public CreateAppRegistrationCommandValidator()
    {
        RuleFor(c => c.Name)
            .NotEmpty()
            .MaximumLength(150);

        RuleFor(c => c.Description)
            .MaximumLength(500);

        RuleFor(c => c.ExpirationUtc)
            .Must(exp => exp is null || exp > DateTimeOffset.UtcNow)
            .WithMessage("Expiration must be in the future.");

        RuleForEach(c => c.Scopes)
            .NotEmpty()
            .MaximumLength(100);
    }
}
