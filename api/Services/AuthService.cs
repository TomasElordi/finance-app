using api.Data;
using api.DTOs;
using api.Exceptions;
using api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.Security.Cryptography;
using System.Text;

namespace api.Services;

public class AuthService(AppDbContext db, IJwtService jwtService) : IAuthService
{
    public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto dto)
    {
        var email = NormalizeEmail(dto.Email);
        // Emails are compared without case, so "Ana@x.com" can't open a second account
        // next to "ana@x.com". Older rows may still have mixed case, hence lower() here.
        if (await db.Users.AnyAsync(u => u.Email.ToLower() == email))
            throw new DuplicateEmailException("Email already exists.");

        var hasher = new PasswordHasher<string>();
        var hashed = hasher.HashPassword(null!, dto.Password);

        var user = new User { Id = Guid.NewGuid(), Name = dto.Name.Trim(), Email = email, Password = hashed };
        db.Users.Add(user);

        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException pgEx && pgEx.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            throw new DuplicateEmailException("Email already exists.");
        }

        return await IssueTokensAsync(user);
    }

    // Same message for an unknown email and a wrong password, so the login can't be used
    // to find out who has an account.
    public const string InvalidCredentialsMessage = "Email o contraseña incorrectos.";

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto dto)
    {
        var user = await ValidateCredentialsAsync(dto.Email, dto.Password)
            ?? throw new ValidationException(InvalidCredentialsMessage);
        return await IssueTokensAsync(user);
    }

    public async Task<AuthResponseDto> RefreshAsync(string refreshToken)
    {
        var tokenHash = HashToken(refreshToken);
        var stored = await db.RefreshToken
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Token == tokenHash);

        if (stored == null || stored.ExpirationDate < DateTime.UtcNow)
            throw new ValidationException("Invalid or expired refresh token.");

        db.RefreshToken.Remove(stored);
        return await IssueTokensAsync(stored.User);
    }

    // Refresh tokens are stored only as a SHA-256 hash (like the OAuth ones), so a leak
    // of the table doesn't hand out working tokens.
    private async Task<AuthResponseDto> IssueTokensAsync(User user)
    {
        var tokens = jwtService.GenerateTokens(user.Id, user.Name, user.Email);
        db.RefreshToken.Add(new RefreshToken { Id = Guid.NewGuid(), UserId = user.Id, Token = HashToken(tokens.RefreshToken), ExpirationDate = DateTime.UtcNow.AddDays(30) });
        await db.SaveChangesAsync();

        return new AuthResponseDto { AccessToken = tokens.AccessToken, RefreshToken = tokens.RefreshToken, User = new UserResponseDto { Id = user.Id, Name = user.Name, Email = user.Email } };
    }

    private static string HashToken(string token) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));

    private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();

    // Case-insensitive lookup. If old data has two accounts that differ only in case,
    // the exact spelling decides; otherwise neither matches.
    private async Task<User?> FindByEmailAsync(string email)
    {
        var normalized = NormalizeEmail(email);
        var matches = await db.Users.Where(u => u.Email.ToLower() == normalized).Take(2).ToListAsync();
        return matches.Count == 1 ? matches[0] : matches.FirstOrDefault(u => u.Email == email.Trim());
    }

    public async Task<User?> ValidateCredentialsAsync(string email, string password)
    {
        await EnsureNotLockedAsync(email);
        var user = await FindByEmailAsync(email);
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
