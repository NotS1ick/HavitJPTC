using System.Diagnostics;
using Havit.Models;
using Microsoft.AspNetCore.Mvc;
using Havit.Services.Managers;

namespace Havit.Controllers;

public class BaseController : Controller
{
    protected DataTrackerModel GetDataTrackerModel()
    {
        return new DataTrackerModel()
        {
            IsAuthenticated = User.Identity?.IsAuthenticated ?? false,
            CurrentUsername = User.Identity?.Name,
            CurrentUserRole = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Role)?.Value
        };
    }
}