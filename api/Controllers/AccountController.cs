namespace api.Controllers;

using System.Security.Claims;
using api.Data;
using api.DTOs;
using api.Models;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Authorize]
[Route("api/account")]
public class AccountController : ControllerBase
{
     private readonly AppDbContext _appDbContext;

    public AccountController(AppDbContext appDbContext)
    {
        _appDbContext = appDbContext;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<GetAccountsResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get()
    {
        try{
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            var accounts = await _appDbContext.Accounts
                            .Where(a => a.UserId  == userId)
                            .ToListAsync();
            var accountsDto = accounts.Adapt<List<AccountResponseDto>>();
            return Ok(ApiResponse<GetAccountsResponseDto>.Ok(new GetAccountsResponseDto{Accounts = accountsDto}));
        }
        catch(Exception)
        {
            return StatusCode(500, ApiResponse<AccountResponseDto>.Fail("Internal Server Error"));
        }
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<AccountResponseDto>),StatusCodes.Status200OK)]
    public async Task<IActionResult> Post([FromBody] PostAccountRequestDto postAccountRequestDto)
    {
        try{
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            var account = new Account{ Id = Guid.NewGuid(), Name = postAccountRequestDto.Name, Nature = postAccountRequestDto.Nature, UserId = userId };

            _appDbContext.Accounts.Add(account);
            await _appDbContext.SaveChangesAsync();

            return Ok(ApiResponse<AccountResponseDto>.Ok(new AccountResponseDto{Id = account.Id, Name= account.Name, Nature = account.Nature}));
        }
        catch(Exception)
        {
            return StatusCode(500, ApiResponse<AccountResponseDto>.Fail("Internal Server Error"));
        }
    }
}