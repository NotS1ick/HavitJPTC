namespace Havit.Services.Interfaces;

public interface IUserStore
{
    bool AddUser(string username, string password);
    bool ValidateUser(string username, string password);
    bool UserExists(string username);
    bool IsUserAdmin(string username);
    string? GetUserRole(string username);
}