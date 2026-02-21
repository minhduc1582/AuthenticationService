using Application.AppRegistrations.Commands.CreateAppRegistration;
using Application.AppRegistrations.Commands.RegenerateClientSecret;
using Application.AppRegistrations.Commands.SetAppRegistrationExpiration;
using Application.AppRegistrations.Commands.ToggleAppRegistrationStatus;
using Application.AppRegistrations.Dtos;
using Application.AppRegistrations.Queries.GetAppRegistrationById;
using Application.AppRegistrations.Queries.GetAppRegistrations;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthenticationAPI.Controllers;

[Authorize]
[ApiController]
[Route("api/apps")]
public class AppRegistrationsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AppRegistrationsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<AppRegistrationDto>>> GetApps(CancellationToken cancellationToken)
    {
        try
        {
            var apps = await _mediator.Send(new GetAppRegistrationsQuery(), cancellationToken);
            return Ok(apps);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AppRegistrationDetailsDto>> GetApp(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var app = await _mediator.Send(new GetAppRegistrationByIdQuery(id), cancellationToken);
            return Ok(app);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    [HttpPost]
    public async Task<ActionResult<CreateAppRegistrationResult>> CreateApp(
        [FromBody] CreateAppRegistrationRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateAppRegistrationCommand(
            request.Name,
            request.Description,
            request.Scopes?.ToArray(),
            request.ExpirationUtc);

        try
        {
            var result = await _mediator.Send(command, cancellationToken);
            return CreatedAtAction(nameof(GetApp), new { id = result.AppRegistration.Id }, result);
        }
        catch (ValidationException ex)
        {
            return BadRequest(new { errors = ex.Errors.Select(e => e.ErrorMessage) });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    [HttpPost("{id:guid}/secret")]
    public async Task<ActionResult<ClientSecretResponseDto>> RegenerateSecret(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _mediator.Send(new RegenerateClientSecretCommand(id), cancellationToken);
            return Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    [HttpPatch("{id:guid}/expiration")]
    public async Task<IActionResult> SetExpiration(Guid id, [FromBody] SetExpirationRequest request, CancellationToken cancellationToken)
    {
        try
        {
            await _mediator.Send(new SetAppRegistrationExpirationCommand(id, request.ExpirationUtc), cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> ToggleStatus(Guid id, [FromBody] ToggleStatusRequest request, CancellationToken cancellationToken)
    {
        try
        {
            await _mediator.Send(new ToggleAppRegistrationStatusCommand(id, request.IsEnabled), cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }
}

public record CreateAppRegistrationRequest(
    string Name,
    string? Description,
    ICollection<string>? Scopes,
    DateTimeOffset? ExpirationUtc);

public record SetExpirationRequest(DateTimeOffset? ExpirationUtc);

public record ToggleStatusRequest(bool IsEnabled);
