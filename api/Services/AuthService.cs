using api.Data;
using api.DTOs;
using api.Exceptions;
using api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace api.Services;

public class AuthService(AppDbContext db, IJwtService jwtService) : IAuthService
{
    public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto dto)
    {
        var hasher = new PasswordHasher<string>();
        var hashed = hasher.HashPassword(null!, dto.Password);

        var user = new User { Id = Guid.NewGuid(), Name = dto.Name, Email = dto.Email, Password = hashed };
        db.Users.Add(user);

        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException pgEx && pgEx.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            throw new DuplicateEmailException("Email already exists.");
        }

        var tokens = jwtService.GenerateTokens(user.Id, user.Name, user.Email);
        db.RefreshToken.Add(new RefreshToken { Id = Guid.NewGuid(), UserId = user.Id, Token = tokens.RefreshToken, ExpirationDate = DateTime.UtcNow.AddDays(30) });
        await db.SaveChangesAsync();

        return new AuthResponseDto { AccessToken = tokens.AccessToken, RefreshToken = tokens.RefreshToken, User = new UserResponseDto { Id = user.Id, Name = user.Name, Email = user.Email } };
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto dto)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == dto.Email)
            ?? throw new NotFoundException("User not exists.");

        var hasher = new PasswordHasher<string>();
        var result = hasher.VerifyHashedPassword(null!, user.Password, dto.Password);

        if (result != PasswordVerificationResult.Success)
            throw new ValidationException("Wrong password.");

        var tokens = jwtService.GenerateTokens(user.Id, user.Name, user.Email);
        db.RefreshToken.Add(new RefreshToken { Id = Guid.NewGuid(), UserId = user.Id, Token = tokens.RefreshToken, ExpirationDate = DateTime.UtcNow.AddDays(30) });
        await db.SaveChangesAsync();

        return new AuthResponseDto { AccessToken = tokens.AccessToken, RefreshToken = tokens.RefreshToken, User = new UserResponseDto { Id = user.Id, Name = user.Name, Email = user.Email } };
    }

    public async Task<AuthResponseDto> RefreshAsync(string refreshToken)
    {
        var stored = await db.RefreshToken
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Token == refreshToken);

        if (stored == null || stored.ExpirationDate < DateTime.UtcNow)
            throw new ValidationException("Invalid or expired refresh token.");

        var user = stored.User;
        db.RefreshToken.Remove(stored);

        var tokens = jwtService.GenerateTokens(user.Id, user.Name, user.Email);
        db.RefreshToken.Add(new RefreshToken { Id = Guid.NewGuid(), UserId = user.Id, Token = tokens.RefreshToken, ExpirationDate = DateTime.UtcNow.AddDays(30) });
        await db.SaveChangesAsync();

        return new AuthResponseDto { AccessToken = tokens.AccessToken, RefreshToken = tokens.RefreshToken, User = new UserResponseDto { Id = user.Id, Name = user.Name, Email = user.Email } };
    }

    public async Task LogoutAsync(Guid userId)
    {
        var tokens = await db.RefreshToken.Where(r => r.UserId == userId).ToListAsync();
        db.RefreshToken.RemoveRange(tokens);
        await db.SaveChangesAsync();
    }
}
