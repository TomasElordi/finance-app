using api.Data;
using api.DTOs;
using api.Models;
using api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _appDbContext;
    private readonly IJwtService _jwtService;

    public AuthController(AppDbContext appDbContext, IJwtService jwtService)
    {
        _appDbContext = appDbContext;
        _jwtService = jwtService;
    }

    [HttpPost("register")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Post([FromBody] RegisterRequestDto registerRequestDto)
    {
        try
        {
            //Hash password 
            var hasher = new PasswordHasher<string>();
            var hashed = hasher.HashPassword(null!, registerRequestDto.Password);

            //Create user
            var user = new User{ Id = Guid.NewGuid(), Name = registerRequestDto.Name, Email= registerRequestDto.Email, Password = hashed };

            //Insert on db
            _appDbContext.Users.Add(user);
            await _appDbContext.SaveChangesAsync();

            //Generate tokens
            var tokens = _jwtService.GenerateTokens(user.Id, user.Name, user.Email);

            var refreshToken = new RefreshToken{ Id = Guid.NewGuid(), UserId = user.Id, Token= tokens.RefreshToken , ExpirationDate = DateTime.UtcNow.AddDays(30)};
            _appDbContext.RefreshToken.Add(refreshToken);
            await _appDbContext.SaveChangesAsync();

            //Return response
            return Ok(ApiResponse<AuthResponseDto>.Ok(new AuthResponseDto{AccessToken = tokens.AccessToken, RefreshToken= tokens.RefreshToken,  User = new UserResponseDto { Id = user.Id, Name =  user.Name, Email = user.Email } }));
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException pgEx && pgEx.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            return BadRequest(ApiResponse<UserResponseDto>.Fail("Email already exists."));
        }
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Post([FromBody] LoginRequestDto loginRequestDto)
    {
        //Find on db
        User? user = await _appDbContext.Users.FirstOrDefaultAsync(u => u.Email == loginRequestDto.Email);

        if(user == null)
        {
            return BadRequest(ApiResponse<UserResponseDto>.Fail("User not exists."));
        };
        //Hash password 
        var hasher = new PasswordHasher<string>();
        var result = hasher.VerifyHashedPassword(null!, user.Password, loginRequestDto.Password);

        if(result != PasswordVerificationResult.Success)
        {
            return BadRequest(ApiResponse<UserResponseDto>.Fail("Wrong password."));
        }

        //Generate tokens
        var tokens = _jwtService.GenerateTokens(user.Id, user.Name, user.Email);

        var refreshToken = new RefreshToken{ Id = Guid.NewGuid(), UserId = user.Id, Token= tokens.RefreshToken , ExpirationDate = DateTime.UtcNow.AddDays(30)};
        _appDbContext.RefreshToken.Add(refreshToken);
        await _appDbContext.SaveChangesAsync();

        //Return response
         return Ok(ApiResponse<AuthResponseDto>.Ok(new AuthResponseDto{AccessToken = tokens.AccessToken, RefreshToken= tokens.RefreshToken,  User = new UserResponseDto { Id = user.Id, Name =  user.Name, Email = user.Email } }));
        
    }
}
