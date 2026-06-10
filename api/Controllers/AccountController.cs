using System.Security.Claims;
using api.DTOs;
using api.Exceptions;
using api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers;

[ApiController]
[Authorize]
[Route("api/account")]
public class AccountController(ILogger<AccountController> logger, IAccountService accountService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<GetAccountsResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get()
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            logger.LogInformation("GET Accounts by User: {User}.", userId);
            var accounts = await accountService.GetAccountsAsync(userId);
            return Ok(ApiResponse<GetAccountsResponseDto>.Ok(new GetAccountsResponseDto { Accounts = accounts }));
        }
        catch (Exception ex)
        {
            logger.LogInformation(ex, "Internal Server Error on GET Accounts");
            return StatusCode(500, ApiResponse<GetAccountsResponseDto>.Fail("Internal Server Error"));
        }
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<AccountResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Post([FromBody] PostAccountRequestDto dto)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            logger.LogInformation("POST Account by User: {User}. Request: {@Request}", userId, dto);
            var account = await accountService.CreateAccountAsync(userId, dto);
            return Ok(ApiResponse<AccountResponseDto>.Ok(account));
        }
        catch (Exception ex)
        {
            logger.LogInformation(ex, "Internal Server Error on POST Account. Request: {@Request}", dto);
            return StatusCode(500, ApiResponse<AccountResponseDto>.Fail("Internal Server Error"));
        }
    }

    [HttpDelete]
    public async Task<IActionResult> Delete([FromQuery] Guid Id)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            logger.LogInformation("DELETE Account with Id: {Id} by User: {User}", Id, userId);
            var found = await accountService.DeleteAccountAsync(userId, Id);
            if (!found)
            {
                logger.LogInformation("Account with Id: {Id} not exists.", Id);
                return BadRequest(ApiResponse<AccountResponseDto>.Fail("Account not exists."));
            }
            return NoContent();
        }
        catch (ForbiddenException ex)
        {
            logger.LogWarning(ex, "Unauthorized for delete account {Id}.", Id);
            return Unauthorized(ApiResponse<AccountResponseDto>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            logger.LogInformation(ex, "Internal Server Error on DELETE Account. Request: {@Request}", Id);
            return StatusCode(500, ApiResponse<AccountResponseDto>.Fail("Internal Server Error"));
        }
    }
}
