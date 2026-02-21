using System.Security.Claims;
using Application.Authorization;
using Microsoft.AspNetCore.Identity;

namespace AuthenticationAPI
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = IdentityConstants.ApplicationScheme;
                    options.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
                    options.DefaultSignInScheme = IdentityConstants.ApplicationScheme;
                });

            services.AddAuthorization(options =>
            {
                options.AddPolicy(AuthorizationPolicies.RequireAdminRole,
                    policy => policy.RequireRole(RoleNames.Admin));

                options.AddPolicy(AuthorizationPolicies.ManageUsers,
                    policy => policy.RequireAssertion(context =>
                        context.User.IsInRole(RoleNames.Admin) ||
                        context.User.HasClaim("permission", "manage_users")));
            });

            return services;
        }
    }
}
