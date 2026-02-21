namespace Domain.AppRegistrations;

public class Scope
{
    private Scope()
    {
    }

    public Scope(string name, string description, bool isDefault = false)
    {
        Name = name;
        Description = description;
        IsDefault = isDefault;
    }

    public Scope(Guid id, string name, string description, bool isDefault = false)
        : this(name, description, isDefault)
    {
        Id = id;
    }

    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public bool IsDefault { get; private set; }
    public bool IsActive { get; private set; } = true;

    public void Disable()
    {
        IsActive = false;
    }

    public void Enable()
    {
        IsActive = true;
    }
}
