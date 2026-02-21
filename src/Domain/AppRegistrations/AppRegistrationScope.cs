namespace Domain.AppRegistrations;

public class AppRegistrationScope
{
    private AppRegistrationScope()
    {
    }

    public AppRegistrationScope(Guid appRegistrationId, Guid scopeId, string scopeName)
    {
        AppRegistrationId = appRegistrationId;
        ScopeId = scopeId;
        ScopeName = scopeName;
    }

    public Guid AppRegistrationId { get; private set; }
    public Guid ScopeId { get; private set; }
    public string ScopeName { get; private set; } = string.Empty;

    public AppRegistration? AppRegistration { get; private set; }
    public Scope? Scope { get; private set; }
}
