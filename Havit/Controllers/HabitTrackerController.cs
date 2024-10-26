using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Havit.Models;
using Microsoft.AspNetCore.Authorization;

namespace Havit.Controllers;

[Authorize]
public class HabitTrackerController : Controller
{
    private readonly ILogger<HabitTrackerController> _logger;

    public HabitTrackerController(ILogger<HabitTrackerController> logger)
    {
        _logger = logger;
    }
    
    public IActionResult Index()
    {
        return View();
    }
    
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}