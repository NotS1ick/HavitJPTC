using System.Security.Claims;
using Havit.Services.Interfaces;
using Havit.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;


// Probably should've stored the users in database, but since I've made it in this way and I'm lazy and don't want to deal with DB's. So It'll do for now.
[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
public class AuthController : Controller
{
    private const string SessionKeyUser = "Username";
    private readonly IUserStore _userStore;

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
    
    [HttpGet]
    public IActionResult CheckAuthState()
    {
        if (User.Identity?.IsAuthenticated == true)
            return Ok(new { isAuthenticated = true, username = User.Identity.Name });
        return Ok(new { isAuthenticated = false });
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginVM model)
    {
        if (ModelState.IsValid)
        {
            if (_userStore.UserExists(model.Username))
                if (_userStore.ValidateUser(model.Username, model.Password))
                {
                    var role = _userStore.GetUserRole(model.Username) ?? "User";

                    var claims = new List<Claim>
                    {
                        new(ClaimTypes.Name, model.Username),
                        new(ClaimTypes.Role, role)
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
                    
                    return RedirectToAction("Index", "Home");
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

            var role = _userStore.GetUserRole(model.Username) ?? "User";

            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, model.Username),
                new(ClaimTypes.Role, role)
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

            TempData["RequiresRefresh"] = true;
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