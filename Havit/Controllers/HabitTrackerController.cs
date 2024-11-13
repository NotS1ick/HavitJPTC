using System.Diagnostics;
using Havit.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Havit.Controllers;

[Authorize(Roles = "User,Admin")]
public class HabitTrackerController : Controller
{
    private readonly ILogger<HabitTrackerController> _logger;

    public HabitTrackerController(ILogger<HabitTrackerController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        foreach (var claim in User.Claims) _logger.LogInformation($"Claim Type: {claim.Type}, Value: {claim.Value}");
        if (!User.IsInRole("User") && !User.IsInRole("Admin"))
        {
            _logger.LogWarning("User is not in the required role to access HabitTracker.");
            return RedirectToAction("AccessDenied", "Auth");
        }

        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}