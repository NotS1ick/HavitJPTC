using System.Collections.Concurrent;
using Havit.Services.Interfaces;
using Havit.Services.Utilities;

namespace Havit.Services.Implementations;

public class InMemoryUserStore : IUserStore
{
    private readonly IPasswordHasher _passwordHasher;
    private readonly ConcurrentDictionary<string, (string PasswordHash, string Role, string UserId)> _users = new();

    public InMemoryUserStore(IPasswordHasher passwordHasher)
    {
        _passwordHasher = passwordHasher;
        
        AddUserWithId("admin", "Admin@123", true, "admin-" + Guid.NewGuid().ToString()); //There is no admin user features, because there was no time - lousy excuse from me
    }

    public string? AddUser(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            return null;
        }

        var userId = "user-" + Guid.NewGuid().ToString();
        return AddUserWithId(username, password, false, userId) ? userId : null;
    }

    private bool AddUserWithId(string username, string password, bool isAdmin, string userId)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            return false;
        }

        var hashedPassword = _passwordHasher.HashPassword(password);
        var role = isAdmin ? "Admin" : "User";
        return _users.TryAdd(username.ToLower(), (hashedPassword, role, userId));
    }

    public bool ValidateUser(string username, string password)
    {
        if (_users.TryGetValue(username, out var userInfo))
            return _passwordHasher.VerifyPassword(userInfo.PasswordHash, password);
        return false;
    }

    public string? GetUserId(string username)
    {
        if (_users.TryGetValue(username, out var userInfo))
            return userInfo.UserId;
        return null;
    }

    public bool UserExists(string username)
    {
        return _users.ContainsKey(username);
    }

    public string? GetUserRole(string username)
    {
        if (_users.TryGetValue(username, out var userInfo)) 
            return userInfo.Role;
        return null;
    }
}