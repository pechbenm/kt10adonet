using KT10.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using KT10.Data;

namespace KT10.Services;

public enum UserOpStatus { Ok, NotFound, UsernameTaken, EmailTaken }

public record UserOpResult(UserOpStatus Status, AppUser? User = null);

public interface IUserService
{
    Task<List<AppUser>> GetAllAsync();
    Task<AppUser?> GetByIdAsync(int id);
    Task<UserOpResult> CreateAsync(string username, string email, string password);
    Task<UserOpResult> UpdateAsync(int id, string username, string email, string? password);
    Task<UserOpResult> DeleteAsync(int id);
}

public class UserService : IUserService
{
    private static readonly SemaphoreSlim Gate = new(1, 1);

    private readonly AppDbContext _db;
    private readonly IPasswordHasher<AppUser> _hasher;

    public UserService(AppDbContext db, IPasswordHasher<AppUser> hasher)
    {
        _db = db;
        _hasher = hasher;
    }

    public Task<List<AppUser>> GetAllAsync()
        => _db.Users.AsNoTracking().OrderBy(u => u.Id).ToListAsync();

    public Task<AppUser?> GetByIdAsync(int id)
        => _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);

    public async Task<UserOpResult> CreateAsync(string username, string email, string password)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();

        await Gate.WaitAsync();
        try
        {
            var conflict = await FindConflictAsync(username, normalizedEmail, excludeId: 0);
            if (conflict != UserOpStatus.Ok) return new(conflict);

            var user = new AppUser
            {
                Username = username,
                Email = normalizedEmail,
                CreatedAt = DateTime.UtcNow
            };
            user.PasswordHash = _hasher.HashPassword(user, password);

            _db.Users.Add(user);
            await _db.SaveChangesAsync();
            return new(UserOpStatus.Ok, user);
        }
        finally { Gate.Release(); }
    }

    public async Task<UserOpResult> UpdateAsync(int id, string username, string email, string? password)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();

        await Gate.WaitAsync();
        try
        {
            var user = await _db.Users.FindAsync(id);
            if (user is null) return new(UserOpStatus.NotFound);

            var conflict = await FindConflictAsync(username, normalizedEmail, excludeId: id);
            if (conflict != UserOpStatus.Ok) return new(conflict);

            user.Username = username;
            user.Email = normalizedEmail;
            if (!string.IsNullOrEmpty(password))
                user.PasswordHash = _hasher.HashPassword(user, password);

            await _db.SaveChangesAsync();
            return new(UserOpStatus.Ok, user);
        }
        finally { Gate.Release(); }
    }

    public async Task<UserOpResult> DeleteAsync(int id)
    {
        var user = await _db.Users.FindAsync(id);
        if (user is null) return new(UserOpStatus.NotFound);

        _db.Users.Remove(user);
        await _db.SaveChangesAsync();
        return new(UserOpStatus.Ok);
    }

    // excludeId: при обновлении саму запись из проверки исключаем
    private async Task<UserOpStatus> FindConflictAsync(string username, string normalizedEmail, int excludeId)
    {
        var lowerName = username.ToLower();

        if (await _db.Users.AnyAsync(u => u.Id != excludeId && u.Username.ToLower() == lowerName))
            return UserOpStatus.UsernameTaken;

        if (await _db.Users.AnyAsync(u => u.Id != excludeId && u.Email == normalizedEmail))
            return UserOpStatus.EmailTaken;

        return UserOpStatus.Ok;
    }
}