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

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete([FromRoute] Guid id)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            logger.LogInformation("DELETE Account with Id: {Id} by User: {User}", id, userId);
            var found = await accountService.DeleteAccountAsync(userId, id);
            if (!found)
            {
                logger.LogInformation("Account with Id: {Id} not exists.", id);
                return NotFound(ApiResponse<AccountResponseDto>.Fail("Account not exists."));
            }
            return NoContent();
        }
        catch (ForbiddenException ex)
        {
            logger.LogWarning(ex, "Forbidden: delete account {Id}.", id);
            return StatusCode(403, ApiResponse<AccountResponseDto>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            logger.LogInformation(ex, "Internal Server Error on DELETE Account. Request: {@Request}", id);
            return StatusCode(500, ApiResponse<AccountResponseDto>.Fail("Internal Server Error"));
        }
    }
}
