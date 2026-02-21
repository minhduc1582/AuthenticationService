using Application;
using Application.Authorization;
using Infrastructure;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using AuthenticationAPI.Security;

namespace AuthenticationAPI.Controllers;

[ApiController]
[Route("api/account")]
public class AccountController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IAntiforgery _antiforgery;
    private readonly ILogger<AccountController> _logger;
    private readonly string _defaultRedirectUrl;
    private readonly HashSet<string> _allowedRedirectOrigins;
    private readonly int _rememberMeDays;

    public AccountController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IAntiforgery antiforgery,
        IConfiguration configuration,
        ILogger<AccountController> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _antiforgery = antiforgery;
        _logger = logger;

        _defaultRedirectUrl = configuration.GetValue<string>("Authentication:DefaultRedirectUrl") ?? "/";

        var configuredOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();
        _allowedRedirectOrigins = configuredOrigins is { Length: > 0 }
            ? configuredOrigins.Select(origin => origin.TrimEnd('/'))
                .ToHashSet(StringComparer.OrdinalIgnoreCase)
            : new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        _rememberMeDays = configuration.GetSection("Authentication:Cookie")
            .GetValue<int?>("RememberMeDays") ?? 14;
    }

    [AllowAnonymous]
    [HttpGet("antiforgery")]
    public IActionResult GetAntiforgeryToken()
    {
        var tokens = _antiforgery.GetAndStoreTokens(HttpContext);
        Response.Headers["X-CSRF-TOKEN"] = tokens.RequestToken!;
        return Ok(new { token = tokens.RequestToken });
    }

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<ActionResult<AuthResultDto>> Register([FromBody] RegisterDto model)
    {
        if (!await _antiforgery.TryValidateRequestAsync(HttpContext, _logger))
        {
            return BadRequest(AuthResultDto.Fail("Invalid or missing anti-forgery token."));
        }

        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var existingUser = await _userManager.FindByEmailAsync(model.Email);
        if (existingUser is not null)
        {
            return Conflict(AuthResultDto.Fail("Email is already registered."));
        }

        var user = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email,
            FirstName = model.FirstName,
            LastName = model.LastName
        };

        var result = await _userManager.CreateAsync(user, model.Password);
        if (!result.Succeeded)
        {
            return BadRequest(AuthResultDto.Fail("Registration failed.", result.Errors.Select(e => e.Description)));
        }

        var roleResult = await _userManager.AddToRoleAsync(user, RoleNames.User);
        if (!roleResult.Succeeded)
        {
            _logger.LogWarning("Failed to add user {Email} to default role. Errors: {Errors}", model.Email,
                string.Join(", ", roleResult.Errors.Select(e => e.Description)));
        }

        _logger.LogInformation("User {Email} registered successfully.", model.Email);

        return Ok(AuthResultDto.Ok("Registered successfully.", await BuildUserDto(user), _defaultRedirectUrl));
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<AuthResultDto>> Login([FromBody] LoginDto model)
    {
        if (!await _antiforgery.TryValidateRequestAsync(HttpContext, _logger))
        {
            return BadRequest(AuthResultDto.Fail("Invalid or missing anti-forgery token."));
        }

        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user is null || user.IsDeleted)
        {
            return Unauthorized(AuthResultDto.Fail("Invalid credentials."));
        }

        var signInResult = await _signInManager.CheckPasswordSignInAsync(user, model.Password, lockoutOnFailure: true);
        if (signInResult.IsLockedOut)
        {
            return StatusCode(StatusCodes.Status423Locked, AuthResultDto.Fail("Account is locked. Please try again later."));
        }

        if (!signInResult.Succeeded)
        {
            return Unauthorized(AuthResultDto.Fail("Invalid credentials."));
        }

        var authProperties = new AuthenticationProperties
        {
            IsPersistent = model.RememberMe,
            AllowRefresh = true
        };

        if (model.RememberMe)
        {
            authProperties.ExpiresUtc = DateTimeOffset.UtcNow.AddDays(_rememberMeDays);
        }

        await _signInManager.SignInAsync(user, authProperties);

        user.LastLoginAt = DateTimeOffset.UtcNow;
        await _userManager.UpdateAsync(user);

        var redirect = ResolveRedirectUrl(model.RedirectUrl);
        _logger.LogInformation("User {Email} logged in successfully.", model.Email);

        return Ok(AuthResultDto.Ok("Login successful.", await BuildUserDto(user), redirect));
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<ActionResult<AuthResultDto>> Logout()
    {
        if (!await _antiforgery.TryValidateRequestAsync(HttpContext, _logger))
        {
            return BadRequest(AuthResultDto.Fail("Invalid or missing anti-forgery token."));
        }
        await _signInManager.SignOutAsync();
        _logger.LogInformation("User {UserId} logged out.", _userManager.GetUserId(User));
        return Ok(AuthResultDto.Ok("Logout successful.", null, _defaultRedirectUrl));
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<CurrentUserDto>> Me()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            return Unauthorized();
        }

        return Ok(await BuildUserDto(user));
    }

    private async Task<CurrentUserDto> BuildUserDto(ApplicationUser user)
    {
        var roles = await _userManager.GetRolesAsync(user);
        return new CurrentUserDto
        {
            Id = user.Id,
            Email = user.Email ?? string.Empty,
            DisplayName = user.DisplayName ?? user.Email,
            EmailConfirmed = user.EmailConfirmed,
            Roles = roles.ToArray()
        };
    }

    private string ResolveRedirectUrl(string? requestedUrl)
    {
        if (!string.IsNullOrWhiteSpace(requestedUrl) &&
            Uri.TryCreate(requestedUrl, UriKind.Absolute, out var uri))
        {
            var requestedOrigin = uri.GetLeftPart(UriPartial.Authority).TrimEnd('/');
            if (_allowedRedirectOrigins.Contains(requestedOrigin))
            {
                return requestedUrl;
            }
        }

        if (!string.IsNullOrWhiteSpace(requestedUrl) &&
            requestedUrl.StartsWith("/", StringComparison.Ordinal))
        {
            return requestedUrl;
        }

        return _defaultRedirectUrl;
    }
}
