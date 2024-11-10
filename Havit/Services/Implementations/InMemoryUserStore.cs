using System.Collections.Concurrent;
using Havit.Services.Interfaces;
using Havit.Services.Utilities;

namespace Havit.Services.Implementations;

public class InMemoryUserStore : IUserStore
{
    private readonly IPasswordHasher _passwordHasher;
    private readonly ConcurrentDictionary<string, (string PasswordHash, string Role)> _users = new();

    public InMemoryUserStore(IPasswordHasher passwordHasher)
    {
        _passwordHasher = passwordHasher;

        AddUser("admin", "Admin@123", true);
    }

    public bool AddUser(string username, string password)
    {
        return AddUser(username, password, false);
    }

    public bool ValidateUser(string username, string password)
    {
        if (_users.TryGetValue(username, out var userInfo))
            return _passwordHasher.VerifyPassword(userInfo.PasswordHash, password);
        return false;
    }

    public bool UserExists(string username)
    {
        return _users.ContainsKey(username);
    }

    public bool IsUserAdmin(string username)
    {
        if (_users.TryGetValue(username, out var userInfo))
            return userInfo.Role.Equals("Admin", StringComparison.OrdinalIgnoreCase);
        return false;
    }

    public string? GetUserRole(string username)
    {
        if (_users.TryGetValue(username, out var userInfo)) return userInfo.Role;
        return null;
    }

    public bool AddUser(string username, string password, bool isAdmin)
    {
        var hashedPassword = _passwordHasher.HashPassword(password);
        var role = isAdmin ? "Admin" : "User";
        return _users.TryAdd(username, (hashedPassword, role));
    }
}