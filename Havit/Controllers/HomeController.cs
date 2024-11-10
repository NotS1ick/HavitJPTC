using System;
using System.Diagnostics;
using Havit.Models;
using Microsoft.AspNetCore.Mvc;
using Havit.Services.Managers;

namespace Havit.Controllers;

// Remove the controller-level cache attribute since we want to control caching at the action level
public class HomeController : BaseController
{
    private readonly IAuthStateService _authStateService;
    
    public HomeController(IAuthStateService authStateService)
    {
        _authStateService = authStateService ?? throw new ArgumentNullException(nameof(authStateService));
    }
    
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Index()
    {
        var viewModel = GetDataTrackerModel();
        return View(viewModel);
    }
    
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult TermsOfService()
    {
        var viewModel = GetDataTrackerModel();
        return View(viewModel);
    }
    
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult ContactUs()
    {
        var viewModel = GetDataTrackerModel();
        return View(viewModel);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}