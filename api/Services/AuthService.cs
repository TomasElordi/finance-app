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

    // Same message for an unknown email and a wrong password, so the login can't be used
    // to find out who has an account.
    public const string InvalidCredentialsMessage = "Email o contraseña incorrectos.";

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto dto)
    {
        var user = await ValidateCredentialsAsync(dto.Email, dto.Password)
            ?? throw new ValidationException(InvalidCredentialsMessage);

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

    public async Task<User?> ValidateCredentialsAsync(string email, string password)
    {
        await EnsureNotLockedAsync(email);
        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == email.Trim());
        // Unknown emails still pay for a hash check, so the response time doesn't tell
        // them apart from a wrong password.
        var verified = new PasswordHasher<string>()
            .VerifyHashedPassword(null!, user?.Password ?? DummyHash.Value, password) != PasswordVerificationResult.Failed;
        if (user is null || !verified)
        {
            await RecordFailureAsync(email);
            return null;
        }
        await ClearFailuresAsync(email);
        return user;
    }

    private static readonly Lazy<string> DummyHash =
        new(() => new PasswordHasher<string>().HashPassword(null!, Guid.NewGuid().ToString()));

    // ---------- Login throttling ----------
    // After MaxFailures failed logins for an email within FailureWindow, further attempts
    // are rejected (even with the right password) until the oldest failure ages out.

    private const int MaxFailures = 5;
    private static readonly TimeSpan FailureWindow = TimeSpan.FromMinutes(15);

    private static string ThrottleKey(string email) => email.Trim().ToLowerInvariant();

    private async Task EnsureNotLockedAsync(string email)
    {
        var key = ThrottleKey(email);
        var since = DateTime.UtcNow - FailureWindow;
        var failures = await db.LoginFailures.CountAsync(f => f.Email == key && f.CreatedAt > since);
        if (failures >= MaxFailures)
            throw new TooManyAttemptsException("Demasiados intentos fallidos. Probá de nuevo en 15 minutos.");
    }

    private async Task RecordFailureAsync(string email)
    {
        var key = ThrottleKey(email);
        var since = DateTime.UtcNow - FailureWindow;
        // Expired rows of every email go too, so guesses at made-up emails don't pile up.
        await db.LoginFailures.Where(f => f.CreatedAt <= since).ExecuteDeleteAsync();
        db.LoginFailures.Add(new LoginFailure { Id = Guid.NewGuid(), Email = key, CreatedAt = DateTime.UtcNow });
        await db.SaveChangesAsync();
    }

    private Task ClearFailuresAsync(string email)
    {
        var key = ThrottleKey(email);
        return db.LoginFailures.Where(f => f.Email == key).ExecuteDeleteAsync();
    }

    public async Task LogoutAsync(Guid userId)
    {
        var tokens = await db.RefreshToken.Where(r => r.UserId == userId).ToListAsync();
        db.RefreshToken.RemoveRange(tokens);
        await db.SaveChangesAsync();
    }
}
