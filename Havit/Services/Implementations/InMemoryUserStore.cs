using System;
using System.Collections.Concurrent;
using Havit.Services.Interfaces;
using Havit.Services.Utilities;

namespace Havit.Services.Implementations
{
    public class InMemoryUserStore : IUserStore
    {
        private readonly ConcurrentDictionary<string, (string PasswordHash, string Role)> _users = new ConcurrentDictionary<string, (string, string)>();
        private readonly IPasswordHasher _passwordHasher;

        public InMemoryUserStore(IPasswordHasher passwordHasher)
        {
            _passwordHasher = passwordHasher;
            
            // Add an admin user
            AddUser("admin", "Admin@123", isAdmin: true);
        }
        
        public bool AddUser(string username, string password)
        {
            return AddUser(username, password, isAdmin: false);
        }
        
        public bool AddUser(string username, string password, bool isAdmin)
        {
            string hashedPassword = _passwordHasher.HashPassword(password);
            string role = isAdmin ? "Admin" : "User";
            return _users.TryAdd(username, (hashedPassword, role));
        }

        public bool ValidateUser(string username, string password)
        {
            if (_users.TryGetValue(username, out var userInfo))
            {
                return _passwordHasher.VerifyPassword(userInfo.PasswordHash, password);
            }
            return false;
        }

        public bool UserExists(string username)
        {
            return _users.ContainsKey(username);
        }

        public bool IsUserAdmin(string username)
        {
            if (_users.TryGetValue(username, out var userInfo))
            {
                return userInfo.Role.Equals("Admin", StringComparison.OrdinalIgnoreCase);
            }
            return false;
        }
        
        public string? GetUserRole(string username)
        {
            if (_users.TryGetValue(username, out var userInfo))
            {
                return userInfo.Role;
            }
            return null;
        }
    }
}