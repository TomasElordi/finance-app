using System.Security.Claims;
using api.DTOs;
using api.Exceptions;
using api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(ILogger<AuthController> logger, IAuthService authService) : ControllerBase
{
    [HttpPost("register")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto dto)
    {
        logger.LogInformation("Register for {Email}", dto.Email);
        try
        {
            var result = await authService.RegisterAsync(dto);
            logger.LogInformation("User {Email} created", dto.Email);
            return Ok(ApiResponse<AuthResponseDto>.Ok(result));
        }
        catch (DuplicateEmailException ex)
        {
            logger.LogWarning(ex, "User with {Email} already exists.", dto.Email);
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
        logger.LogInformation("Login for {Email}", dto.Email);
        try
        {
            var result = await authService.LoginAsync(dto);
            return Ok(ApiResponse<AuthResponseDto>.Ok(result));
        }
        catch (NotFoundException ex)
        {
            logger.LogWarning(ex, "User {Email} not exists", dto.Email);
            return BadRequest(ApiResponse<AuthResponseDto>.Fail(ex.Message));
        }
        catch (ValidationException ex)
        {
            logger.LogWarning(ex, "Wrong password for {Email}", dto.Email);
            return BadRequest(ApiResponse<AuthResponseDto>.Fail(ex.Message));
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

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            logger.LogInformation("Logout for User: {User}", userId);
            await authService.LogoutAsync(userId);
            return NoContent();
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Internal Server Error on Logout");
            return StatusCode(500, ApiResponse<AuthResponseDto>.Fail("Internal Server Error"));
        }
    }
}
