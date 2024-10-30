using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Havit.ViewModels;
using Havit.Services.Interfaces;

public class AuthController : Controller
{
    private readonly IUserStore _userStore;
    private const string SessionKeyUser = "Username";

    public AuthController(IUserStore userStore)
    {
        _userStore = userStore;
    }

    public IActionResult Login()
    {
        return View();
    }

    public IActionResult Register()
    {
        return View();
    }

    public IActionResult AccessDenied()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginVM model)
    {
        if (ModelState.IsValid)
        {
            if (_userStore.UserExists(model.Username))
            {
                if (_userStore.ValidateUser(model.Username, model.Password))
                {
                    // Retrieve the user's role from the store
                    var role = _userStore.GetUserRole(model.Username) ?? "User";

                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, model.Username),
                        new Claim(ClaimTypes.Role, role)
                    };

                    var claimsIdentity = new ClaimsIdentity(
                        claims,
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        ClaimTypes.Name,
                        ClaimTypes.Role);

                    var authProperties = new AuthenticationProperties
                    {
                        IsPersistent = model.RememberMe,
                        ExpiresUtc = DateTimeOffset.UtcNow.AddHours(1)
                    };

                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity),
                        authProperties);

                    HttpContext.Session.SetString(SessionKeyUser, model.Username);

                    // Directly redirect to HabitTracker without immediate authentication check
                    return RedirectToAction("Index", "HabitTracker");
                }
            }

            ModelState.AddModelError("", "Invalid username or password");
        }

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Register(RegisterVM model)
    {
        if (ModelState.IsValid)
        {
            if (_userStore.UserExists(model.Username))
            {
                ModelState.AddModelError("Username", "Username already exists");
                return View(model);
            }

            if (!model.AgreedToTos)
            {
                ModelState.AddModelError("AgreedToTos", "You must agree to the Terms of Service");
                return View(model);
            }

            if (!_userStore.AddUser(model.Username, model.Password))
            {
                ModelState.AddModelError("", "Unable to register user. Please try again.");
                return View(model);
            }

            // Retrieve the user's role from the store
            var role = _userStore.GetUserRole(model.Username) ?? "User";

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, model.Username),
                new Claim(ClaimTypes.Role, role)
            };

            var claimsIdentity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme,
                ClaimTypes.Name,
                ClaimTypes.Role);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            HttpContext.Session.SetString(SessionKeyUser, model.Username);

            return RedirectToAction("Index", "HabitTracker");
        }

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        HttpContext.Session.Clear();
        return RedirectToAction("Index", "Home");
    }
}