namespace api.Controllers;

using System.Security.Claims;
using api.Data;
using api.DTOs;
using api.Models;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Authorize]
[Route("api/account")]
public class AccountController : ControllerBase
{
    private readonly ILogger<AccountController> _logger;
    private readonly AppDbContext _appDbContext;

    public AccountController(ILogger<AccountController> logger, AppDbContext appDbContext)
    {
        _logger = logger;
        _appDbContext = appDbContext;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<GetAccountsResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get()
    {
       
        try{
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            _logger.LogInformation("GET Accounts by User: {User}.",userId);
            var accounts = await _appDbContext.Accounts
                            .Where(a => a.UserId  == userId)
                            .ToListAsync();
            var accountsDto = accounts.Adapt<List<AccountResponseDto>>();
            return Ok(ApiResponse<GetAccountsResponseDto>.Ok(new GetAccountsResponseDto{Accounts = accountsDto}));
        }
        catch(Exception ex)
        {
            _logger.LogInformation(ex, "Internal Server Error on GET Accounts" );
            return StatusCode(500, ApiResponse<AccountResponseDto>.Fail("Internal Server Error"));
        }
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<AccountResponseDto>),StatusCodes.Status200OK)]
    public async Task<IActionResult> Post([FromBody] PostAccountRequestDto postAccountRequestDto)
    {
        try{
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            _logger.LogInformation("POST Account by User: {User}. Request: {@Request}", userId,postAccountRequestDto);
            var account = new Account{ Id = Guid.NewGuid(), Name = postAccountRequestDto.Name, Nature = postAccountRequestDto.Nature, UserId = userId };

            _appDbContext.Accounts.Add(account);
            await _appDbContext.SaveChangesAsync();

            return Ok(ApiResponse<AccountResponseDto>.Ok(new AccountResponseDto{Id = account.Id, Name= account.Name, Nature = account.Nature, Balance = account.Balance}));
        }
        catch(Exception ex)
        {
            _logger.LogInformation(ex, "Internal Server Error on POST Account. Request: {@Request}", postAccountRequestDto);
            return StatusCode(500, ApiResponse<AccountResponseDto>.Fail("Internal Server Error"));
        }
    }

    [HttpDelete]
    public async Task<IActionResult> Delete([FromQuery] Guid Id)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            _logger.LogInformation("DELETE Account with Id: {Id} by User: {User}", Id, userId);
            var account = await _appDbContext.Accounts.Where(a => a.Id == Id).FirstOrDefaultAsync();

            if(account == null)
            {
                _logger.LogInformation("Account with Id: {Id} not exists.", Id);
               return BadRequest(ApiResponse<AccountResponseDto>.Fail("Account not exists."));
            }

            if(account.UserId != userId)
            {
                _logger.LogWarning("Unauthorized for delete this account.");
                return Unauthorized(ApiResponse<AccountResponseDto>.Fail("Unauthorized."));
            }

            _appDbContext.Accounts.Remove(account);
            await _appDbContext.SaveChangesAsync();

            return NoContent();
        }
        catch(Exception ex)
        {
             _logger.LogInformation(ex, "Internal Server Error on DELETE Account. Request: {@Request}", Id);
            return StatusCode(500, ApiResponse<AccountResponseDto>.Fail("Internal Server Error"));
        }
    }
}