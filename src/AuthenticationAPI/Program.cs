using System;
using Application;
using AuthenticationAPI;
using Infrastructure;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddInfrastructure();
builder.Services.AddApplication();

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
var cookieSettings = ResolveCookieSettings(builder);

builder.Services.Configure<CookiePolicyOptions>(options =>
{
    // Allow per-cookie SameSite/Secure settings (e.g., SameSite=None auth cookie + non-HttpOnly antiforgery cookie).
    options.MinimumSameSitePolicy = SameSiteMode.Unspecified;
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        if (allowedOrigins.Length == 0)
        {
            policy.AllowAnyOrigin()
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        }
        else
        {
            policy.WithOrigins(allowedOrigins)
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        }
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddServices(builder.Configuration);

var antiforgeryCookieName = CookieConfigurationHelpers.ResolveAntiforgeryCookieName(cookieSettings.Domain);

builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN";
    options.Cookie.Name = antiforgeryCookieName;
    options.Cookie.HttpOnly = false;
    options.Cookie.SameSite = SameSiteMode.None;
    options.Cookie.SecurePolicy = cookieSettings.SecurePolicy;
    options.Cookie.Domain = antiforgeryCookieName.StartsWith("__Host-", StringComparison.Ordinal)
        ? null
        : cookieSettings.Domain;
    options.Cookie.Path = "/";
    // SameSite=None is required whenever the SPA is hosted on a different origin (e.g., Vite dev server on port 5173),
    // because the antiforgery cookie must flow with cross-origin credentialed requests.
});

ConfigureAuthenticationCookies(builder, cookieSettings);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseHttpsRedirection();
app.UseCookiePolicy();
app.UseCors("Frontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

static void ConfigureAuthenticationCookies(WebApplicationBuilder builder, CookieSettings cookieSettings)
{
    builder.Services.ConfigureApplicationCookie(options =>
    {
        options.Cookie.Name = cookieSettings.Name;
        options.Cookie.HttpOnly = true;
        options.Cookie.Path = "/";
        options.Cookie.SameSite = cookieSettings.SameSiteMode;
        options.Cookie.SecurePolicy = cookieSettings.SecurePolicy;
        options.Cookie.Domain = cookieSettings.Domain;
        options.SlidingExpiration = true;
        options.ExpireTimeSpan = TimeSpan.FromMinutes(cookieSettings.ExpireMinutes);
        options.LoginPath = "/api/account/login";
        options.LogoutPath = "/api/account/logout";
        options.AccessDeniedPath = "/api/account/forbidden";
        options.Events ??= new CookieAuthenticationEvents();
        options.Events.OnRedirectToLogin = context =>
        {
            if (IsApiRequest(context.Request))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return Task.CompletedTask;
            }

            context.Response.Redirect(context.RedirectUri);
            return Task.CompletedTask;
        };
        options.Events.OnRedirectToAccessDenied = context =>
        {
            if (IsApiRequest(context.Request))
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                return Task.CompletedTask;
            }

            context.Response.Redirect(context.RedirectUri);
            return Task.CompletedTask;
        };
    });
}

static bool IsApiRequest(HttpRequest request)
{
    return request.Path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase);
}

static CookieSettings ResolveCookieSettings(WebApplicationBuilder builder)
{
    var cookieSection = builder.Configuration.GetSection("Authentication:Cookie");
    var cookieName = cookieSection.GetValue<string>("Name") ?? "__Host-auth";
    var sameSiteValue = cookieSection.GetValue<string>("SameSite");
    var secureSetting = cookieSection.GetValue<bool?>("Secure");
    var domain = CookieConfigurationHelpers.NormalizeDomain(cookieSection.GetValue<string>("Domain"));
    var expirationMinutes = cookieSection.GetValue<int?>("ExpireMinutes") ?? 60;

    var sameSiteMode = Enum.TryParse<SameSiteMode>(sameSiteValue, true, out var parsedSameSite) && parsedSameSite != SameSiteMode.Unspecified
        ? parsedSameSite
        : SameSiteMode.Lax;

    var securePolicy = secureSetting.HasValue
        ? (secureSetting.Value ? CookieSecurePolicy.Always : CookieSecurePolicy.SameAsRequest)
        : builder.Environment.IsDevelopment()
            ? CookieSecurePolicy.SameAsRequest
            : CookieSecurePolicy.Always;

    if (CookieConfigurationHelpers.IsHostPrefixedCookie(cookieName))
    {
        if (!string.IsNullOrEmpty(domain))
        {
            throw new InvalidOperationException(
                $"Authentication cookie '{cookieName}' uses the __Host- prefix and therefore cannot specify a Domain. Remove Authentication:Cookie:Domain or choose a different cookie name.");
        }

        securePolicy = CookieSecurePolicy.Always;
    }

    if (sameSiteMode == SameSiteMode.None)
    {
        // Per browser requirements, SameSite=None cookies must be marked Secure.
        securePolicy = CookieSecurePolicy.Always;
    }

    // Guidance:
    // - SameSite=None (Secure=true) is required when the SPA runs on a different origin/port (e.g., Vite dev server).
    // - SameSite=Lax/Strict (Secure optional) is suitable when the SPA is hosted behind the same domain/reverse proxy as the API.

    return new CookieSettings(cookieName, sameSiteMode, securePolicy, domain, expirationMinutes);
}

internal record CookieSettings(
    string Name,
    SameSiteMode SameSiteMode,
    CookieSecurePolicy SecurePolicy,
    string? Domain,
    int ExpireMinutes);

internal static class CookieConfigurationHelpers
{
    public static bool IsHostPrefixedCookie(string cookieName) =>
        cookieName.StartsWith("__Host-", StringComparison.Ordinal);

    public static string? NormalizeDomain(string? domain) =>
        string.IsNullOrWhiteSpace(domain) ? null : domain.Trim();

    public static string ResolveAntiforgeryCookieName(string? authenticationCookieDomain) =>
        string.IsNullOrEmpty(authenticationCookieDomain)
            ? "__Host-antiforgery"
            : "Authentication.Antiforgery";
}
