using Application.Interfaces;
using Application.Options;
using Infrastructure.Auth;
using Infrastructure.Auth.Secrets;
using Infrastructure.Auth.Tokens;
using Infrastructure.Persistence.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;


namespace Infrastructure
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrastructure(this WebApplicationBuilder builder)
        {
            builder.Configuration.AddJsonFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json"), true, false);
            builder.Configuration.AddJsonFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"appsettings.{CurrentEnvironment()}.json"), true, false);

            var services = builder.Services;
            services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));

            // DbContext
            services.AddDbContext<AuthDbContext>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
            });

            services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
            {
                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<AuthDbContext>()
            .AddDefaultTokenProviders();

            services.AddHttpContextAccessor();
            services.AddScoped<ICurrentUserService, CurrentUserService>();
            services.AddScoped<IAppRegistrationRepository, AppRegistrationRepository>();
            services.AddScoped<IScopeRepository, ScopeRepository>();
            services.AddScoped<IClientSecretGenerator, ClientSecretGenerator>();
            services.AddScoped<IClientSecretHasher, ClientSecretHasher>();
            services.AddScoped<IJwtTokenService, JwtTokenService>();

            return services;
        }

        public static string CurrentEnvironment()
        {
            var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            return string.IsNullOrEmpty(env) ? throw new InvalidOperationException($"Environment variable ASPNETCORE_ENVIRONMENT is not set.") : env;
        }
    }
}
