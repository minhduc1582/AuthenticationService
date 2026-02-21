using Application.AppRegistrations.Commands.IssueClientCredentialsToken;
using Application.AppRegistrations.Dtos;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthenticationAPI.Controllers;

[AllowAnonymous]
[ApiController]
[Route("connect")]
public class TokenController : ControllerBase
{
    private readonly IMediator _mediator;

    public TokenController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("token")]
    public async Task<ActionResult<TokenResponseDto>> IssueToken([FromForm] TokenRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var command = new IssueClientCredentialsTokenCommand(
                request.grant_type,
                request.client_id,
                request.client_secret,
                request.scope);

            var result = await _mediator.Send(command, cancellationToken);
            return Ok(result);
        }
        catch (ValidationException ex)
        {
            return BadRequest(new TokenErrorResponse("invalid_request", ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new TokenErrorResponse("invalid_request", ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new TokenErrorResponse("invalid_client", ex.Message));
        }
        catch (KeyNotFoundException ex)
        {
            return Unauthorized(new TokenErrorResponse("invalid_client", ex.Message));
        }
    }
}

public class TokenRequest
{
    public string grant_type { get; set; } = string.Empty;
    public string client_id { get; set; } = string.Empty;
    public string client_secret { get; set; } = string.Empty;
    public string? scope { get; set; }
}

public record TokenErrorResponse(string error, string error_description);
