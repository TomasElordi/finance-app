using System.Security.Claims;
using api.DTOs;
using api.Exceptions;
using api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(ILogger<AuthController> logger, IAuthService authService, ITokenRevocationService revocation) : ControllerBase
{
    [HttpPost("register")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto dto)
    {
        logger.LogInformation("Register for {Email}", MaskEmail(dto.Email));
        try
        {
            var result = await authService.RegisterAsync(dto);
            logger.LogInformation("User {User} created", result.User.Id);
            return Ok(ApiResponse<AuthResponseDto>.Ok(result));
        }
        catch (DuplicateEmailException ex)
        {
            logger.LogWarning("Register with an email that already exists: {Email}", MaskEmail(dto.Email));
            return BadRequest(ApiResponse<AuthResponseDto>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Internal Server Error on Register");
            return StatusCode(500, ApiResponse<AuthResponseDto>.Fail("Internal Server Error"));
        }
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
    {
        logger.LogInformation("Login for {Email}", MaskEmail(dto.Email));
        try
        {
            var result = await authService.LoginAsync(dto);
            return Ok(ApiResponse<AuthResponseDto>.Ok(result));
        }
        catch (ValidationException ex)
        {
            // Same message whether the email exists or not.
            logger.LogWarning("Failed login for {Email}", MaskEmail(dto.Email));
            return BadRequest(ApiResponse<AuthResponseDto>.Fail(ex.Message));
        }
        catch (TooManyAttemptsException ex)
        {
            logger.LogWarning("Login locked for {Email} after too many failures", MaskEmail(dto.Email));
            return StatusCode(StatusCodes.Status429TooManyRequests, ApiResponse<AuthResponseDto>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Internal Server Error on Login");
            return StatusCode(500, ApiResponse<AuthResponseDto>.Fail("Internal Server Error"));
        }
    }

    [HttpPost("refresh")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequestDto dto)
    {
        logger.LogInformation("Refresh token request");
        try
        {
            var result = await authService.RefreshAsync(dto.RefreshToken);
            return Ok(ApiResponse<AuthResponseDto>.Ok(result));
        }
        catch (ValidationException ex)
        {
            logger.LogWarning(ex.Message);
            return BadRequest(ApiResponse<AuthResponseDto>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Internal Server Error on Refresh");
            return StatusCode(500, ApiResponse<AuthResponseDto>.Fail("Internal Server Error"));
        }
    }

    // Revokes the access token used for this request and the user's web refresh tokens.
    // MCP connections are left alone; they have their own endpoint below.
    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            logger.LogInformation("Logout for User: {User}", userId);
            if (HttpContext.Items[AccessTokenInfo.HttpContextKey] is AccessTokenInfo token)
                await revocation.RevokeAsync(token);
            await authService.LogoutAsync(userId);
            return NoContent();
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Internal Server Error on Logout");
            return StatusCode(500, ApiResponse<AuthResponseDto>.Fail("Internal Server Error"));
        }
    }

    // Disconnects every MCP (OAuth) client of the user: their refresh tokens are deleted
    // and the access tokens they already hold stop working.
    [HttpPost("mcp/revoke")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<RevokeMcpResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> RevokeMcpClients()
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            var clients = await revocation.RevokeMcpClientsAsync(userId);
            logger.LogInformation("MCP clients revoked for User: {User} ({Clients} connected)", userId, clients);
            return Ok(ApiResponse<RevokeMcpResponseDto>.Ok(new RevokeMcpResponseDto { Clients = clients }));
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Internal Server Error on MCP revoke");
            return StatusCode(500, ApiResponse<RevokeMcpResponseDto>.Fail("Internal Server Error"));
        }
    }

    // Logs keep enough to tell accounts apart ("t***@example.com") without storing addresses.
    private static string MaskEmail(string? email)
    {
        var value = email?.Trim() ?? "";
        var at = value.IndexOf('@');
        return at <= 0 ? "***" : $"{value[0]}***{value[at..]}";
    }
}
