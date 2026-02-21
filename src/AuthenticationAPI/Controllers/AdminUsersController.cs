using Application;
using Application.Authorization;
using AuthenticationAPI.Security;
using Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuthenticationAPI.Controllers;

[ApiController]
[Route("api/admin/users")]
[Authorize(Policy = AuthorizationPolicies.ManageUsers)]
public class AdminUsersController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly IAntiforgery _antiforgery;
    private readonly ILogger<AdminUsersController> _logger;

    public AdminUsersController(
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        IAntiforgery antiforgery,
        ILogger<AdminUsersController> logger)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _antiforgery = antiforgery;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<UserResponseDto>>> GetUsers(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] bool includeDeleted = false)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = includeDeleted
            ? _userManager.Users.IgnoreQueryFilters()
            : _userManager.Users;

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(user =>
                (user.Email != null && user.Email.Contains(term)) ||
                (user.FirstName != null && user.FirstName.Contains(term)) ||
                (user.LastName != null && user.LastName.Contains(term)));
        }

        var total = await query.CountAsync();
        var users = await query
            .OrderBy(u => u.Email)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var items = new List<UserResponseDto>(users.Count);
        foreach (var user in users)
        {
            items.Add(await BuildUserResponse(user));
        }

        return Ok(new PagedResult<UserResponseDto>
        {
            Page = page,
            PageSize = pageSize,
            TotalCount = total,
            Items = items
        });
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<UserResponseDto>> GetUser(Guid id)
    {
        var user = await _userManager.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user is null)
        {
            return NotFound();
        }

        return Ok(await BuildUserResponse(user));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<UserResponseDto>> UpdateUser(Guid id, [FromBody] UpdateUserRequest request)
    {
        if (!await _antiforgery.TryValidateRequestAsync(HttpContext, _logger))
        {
            return BadRequest(new { Message = "Invalid or missing anti-forgery token." });
        }

        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var user = await _userManager.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user is null)
        {
            return NotFound();
        }

        if (!string.Equals(user.Email, request.Email, StringComparison.OrdinalIgnoreCase))
        {
            var existing = await _userManager.FindByEmailAsync(request.Email);
            if (existing is not null && existing.Id != user.Id)
            {
                return Conflict(new { Message = "Email address is already in use." });
            }

            var emailResult = await _userManager.SetEmailAsync(user, request.Email);
            if (!emailResult.Succeeded)
            {
                return BadRequest(new { Errors = emailResult.Errors.Select(e => e.Description) });
            }

            var userNameResult = await _userManager.SetUserNameAsync(user, request.Email);
            if (!userNameResult.Succeeded)
            {
                return BadRequest(new { Errors = userNameResult.Errors.Select(e => e.Description) });
            }
        }

        user.FirstName = request.FirstName;
        user.LastName = request.LastName;
        user.EmailConfirmed = request.EmailConfirmed;
        user.LockoutEnabled = request.LockoutEnabled;

        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            return BadRequest(new { Errors = updateResult.Errors.Select(e => e.Description) });
        }

        if (request.LockoutEnd.HasValue)
        {
            await _userManager.SetLockoutEndDateAsync(user, request.LockoutEnd);
        }
        else if (request.UnlockUser)
        {
            await _userManager.SetLockoutEndDateAsync(user, null);
            await _userManager.ResetAccessFailedCountAsync(user);
        }

        _logger.LogInformation("Admin {AdminId} updated user {UserId}.", _userManager.GetUserId(User), user.Id);
        return Ok(await BuildUserResponse(user));
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<UserResponseDto>> SoftDelete(Guid id)
    {
        if (!await _antiforgery.TryValidateRequestAsync(HttpContext, _logger))
        {
            return BadRequest(new { Message = "Invalid or missing anti-forgery token." });
        }

        var user = await _userManager.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user is null)
        {
            return NotFound();
        }

        if (!user.IsDeleted)
        {
            user.IsDeleted = true;
            user.DeletedAt = DateTimeOffset.UtcNow;
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                return BadRequest(new { Errors = result.Errors.Select(e => e.Description) });
            }
        }

        return Ok(await BuildUserResponse(user));
    }

    [HttpPost("{id:guid}/restore")]
    public async Task<ActionResult<UserResponseDto>> Restore(Guid id)
    {
        if (!await _antiforgery.TryValidateRequestAsync(HttpContext, _logger))
        {
            return BadRequest(new { Message = "Invalid or missing anti-forgery token." });
        }

        var user = await _userManager.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user is null)
        {
            return NotFound();
        }

        if (user.IsDeleted)
        {
            user.IsDeleted = false;
            user.DeletedAt = null;
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                return BadRequest(new { Errors = result.Errors.Select(e => e.Description) });
            }
        }

        return Ok(await BuildUserResponse(user));
    }

    [HttpPost("{id:guid}/roles")]
    public async Task<ActionResult<UserResponseDto>> UpdateRoles(Guid id, [FromBody] UpdateUserRolesRequest request)
    {
        if (!await _antiforgery.TryValidateRequestAsync(HttpContext, _logger))
        {
            return BadRequest(new { Message = "Invalid or missing anti-forgery token." });
        }

        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var user = await _userManager.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user is null)
        {
            return NotFound();
        }

        var requestedRoles = request.Roles?
            .Where(r => !string.IsNullOrWhiteSpace(r))
            .Select(r => r.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList() ?? new List<string>();

        var validRoles = await _roleManager.Roles
            .Select(r => r.Name!)
            .ToListAsync();

        var invalidRoles = requestedRoles
            .Where(role => !validRoles.Contains(role, StringComparer.OrdinalIgnoreCase))
            .ToList();

        if (invalidRoles.Count > 0)
        {
            return BadRequest(new { Message = $"Unknown roles: {string.Join(", ", invalidRoles)}" });
        }

        var currentRoles = await _userManager.GetRolesAsync(user);

        var rolesToRemove = currentRoles
            .Where(role => !requestedRoles.Contains(role, StringComparer.OrdinalIgnoreCase))
            .ToArray();

        var rolesToAdd = requestedRoles
            .Where(role => !currentRoles.Contains(role, StringComparer.OrdinalIgnoreCase))
            .ToArray();

        if (rolesToRemove.Length > 0)
        {
            var removeResult = await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
            if (!removeResult.Succeeded)
            {
                return BadRequest(new { Errors = removeResult.Errors.Select(e => e.Description) });
            }
        }

        if (rolesToAdd.Length > 0)
        {
            var addResult = await _userManager.AddToRolesAsync(user, rolesToAdd);
            if (!addResult.Succeeded)
            {
                return BadRequest(new { Errors = addResult.Errors.Select(e => e.Description) });
            }
        }

        return Ok(await BuildUserResponse(user));
    }

    private async Task<UserResponseDto> BuildUserResponse(ApplicationUser user)
    {
        var roles = await _userManager.GetRolesAsync(user);

        return new UserResponseDto
        {
            Id = user.Id,
            Email = user.Email ?? string.Empty,
            FirstName = user.FirstName,
            LastName = user.LastName,
            DisplayName = user.DisplayName,
            EmailConfirmed = user.EmailConfirmed,
            IsDeleted = user.IsDeleted,
            DeletedAt = user.DeletedAt,
            LockoutEnabled = user.LockoutEnabled,
            LockoutEnd = user.LockoutEnd,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt,
            LastLoginAt = user.LastLoginAt,
            Roles = roles.ToArray()
        };
    }
}
