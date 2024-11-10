using Microsoft.AspNetCore.Http;

namespace Havit.Services.Managers;

public interface IAuthStateService
{
    bool IsAuthenticated { get; }
    string? CurrentUsername { get; }
    string? CurrentUserRole { get; }
}