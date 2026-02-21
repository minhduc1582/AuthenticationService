namespace Application.Authorization;

public static class RoleNames
{
    public const string Admin = "Admin";
    public const string User = "User";
}

public static class AuthorizationPolicies
{
    public const string RequireAdminRole = "RequireAdminRole";
    public const string ManageUsers = "ManageUsers";
}
