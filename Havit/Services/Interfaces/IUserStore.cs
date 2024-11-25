namespace Havit.Services.Interfaces;

public interface IUserStore
{
    string? AddUser(string username, string password);
    bool ValidateUser(string username, string password);
    bool UserExists(string username);
    string? GetUserRole(string username);
    string? GetUserId(string username);
}