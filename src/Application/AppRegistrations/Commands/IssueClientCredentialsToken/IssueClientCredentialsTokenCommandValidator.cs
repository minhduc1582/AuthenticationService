using FluentValidation;

namespace Application.AppRegistrations.Commands.IssueClientCredentialsToken;

public class IssueClientCredentialsTokenCommandValidator : AbstractValidator<IssueClientCredentialsTokenCommand>
{
    public IssueClientCredentialsTokenCommandValidator()
    {
        RuleFor(c => c.GrantType)
            .Equal("client_credentials", StringComparer.OrdinalIgnoreCase);

        RuleFor(c => c.ClientId)
            .NotEmpty();

        RuleFor(c => c.ClientSecret)
            .NotEmpty();
    }
}
