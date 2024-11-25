using System.Diagnostics;
using Havit.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace Havit.Controllers;

[Authorize(Roles = "User,Admin")]
public class HabitTrackerController : Controller
{
    private readonly ILogger<HabitTrackerController> _logger;
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly IMemoryCache _cache;
    private const int MAX_FILE_SIZE = 5 * 1024 * 1024;

    private List<(string name, string image)> GetDefaultHabits()
    {
        return new List<(string name, string image)>
        {
            ("Brush Teeth", "/HabitImg/brush.jpg"),
            ("Eat dinner", "/HabitImg/food.png"),
            ("Do homework", "/HabitImg/book.png"),
            ("Go to sleep in time", "/HabitImg/bed.png")
        };
    }
    
    public HabitTrackerController(ILogger<HabitTrackerController> logger,  IWebHostEnvironment webHostEnvironment,  IMemoryCache cache)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _webHostEnvironment = webHostEnvironment ?? throw new ArgumentNullException(nameof(webHostEnvironment));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
    }

    private string GetUserSpecificCookieKey()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? 
                     throw new InvalidOperationException("User ID not found in claims");
        return $"UserHabits_{userId}";
    }

    private string GetUserSpecificInitializedKey()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? 
                     throw new InvalidOperationException("User ID not found in claims");
        return $"HabitsInitialized_{userId}";
    }

    private string GetUserSpecificUploadPath()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? 
                     throw new InvalidOperationException("User ID not found in claims");
        return Path.Combine("uploads", userId);
    }

    public IActionResult Index()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        _logger.LogInformation($"Loading habits for user: {userId}");

        if (!User.IsInRole("User") && !User.IsInRole("Admin"))
        {
            _logger.LogWarning("User is not in the required role to access HabitTracker.");
            return RedirectToAction("AccessDenied", "Auth");
        }

        var initializedKey = GetUserSpecificInitializedKey();
        var habits = GetHabitsFromCookie();
        
        if (Request.Cookies[initializedKey] == null)
        {
            _logger.LogInformation($"Initializing default habits for new user: {userId}");
            habits.Clear();
        
            var defaultImages = GetDefaultHabits();
            habits = defaultImages.Select((habit, index) => new HabitViewModel
            {
                Id = $"habit{index}",
                Name = habit.name,
                ImagePath = habit.image,
                Frequency = "daily",
            }).ToList();
        
            SaveHabitsToCookie(habits);
        
            var options = new CookieOptions
            {
                Expires = DateTime.Now.AddMonths(1),
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict
            };
            Response.Cookies.Append(initializedKey, "true", options);
        }

        return View(habits);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddHabit([FromForm] HabitViewModel model)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (model.ImageFile == null)
                return BadRequest("Image file is required");

            if (model.ImageFile.Length > MAX_FILE_SIZE)
                return BadRequest("File size exceeds maximum limit of 5MB");

            var habits = GetHabitsFromCookie();
            
            var imagePath = await SaveImageFile(model.ImageFile);
            
            var newHabit = new HabitViewModel
            {
                Id = Guid.NewGuid().ToString(),
                Name = model.Name,
                ImagePath = imagePath,
                Frequency = model.Frequency,
                TimesComplete = 0
            };

            habits.Add(newHabit);
            SaveHabitsToCookie(habits);

            _logger.LogInformation($"New habit created: {newHabit.Id}");
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating new habit");
            return StatusCode(500, "Error creating habit");
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateHabit([FromForm] HabitViewModel model)
    {
        try
        {
            _logger.LogInformation($"Updating habit: {model.Id}");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var habits = GetHabitsFromCookie();
            var habit = habits.FirstOrDefault(h => h.Id == model.Id);

            if (habit == null)
                return NotFound();
            
            if (model.ImageFile != null)
            {
                if (model.ImageFile.Length > MAX_FILE_SIZE)
                    return BadRequest("File size exceeds maximum limit of 5MB");

                habit.ImagePath = await SaveImageFile(model.ImageFile);
            }

            habit.Name = model.Name;
            habit.Frequency = model.Frequency;

            SaveHabitsToCookie(habits);
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error updating habit {model.Id}");
            return StatusCode(500, "Error updating habit");
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteHabit(string id)
    {
        try
        {
            var habits = GetHabitsFromCookie();
            var habit = habits.FirstOrDefault(h => h.Id == id);

            if (habit == null)
                return NotFound();
            
            if (!string.IsNullOrEmpty(habit.ImagePath) && 
                !habit.ImagePath.StartsWith("/HabitImg/") && 
                habit.ImagePath.Contains(GetUserSpecificUploadPath()))
            {
                var imagePath = Path.Combine(_webHostEnvironment.WebRootPath, habit.ImagePath.TrimStart('/'));
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
                
                var userUploadDir = Path.Combine(_webHostEnvironment.WebRootPath, GetUserSpecificUploadPath());
                if (Directory.Exists(userUploadDir) && !Directory.EnumerateFiles(userUploadDir).Any())
                {
                    Directory.Delete(userUploadDir);
                }
            }

            habits.Remove(habit);
            SaveHabitsToCookie(habits);
            
            var initializedKey = GetUserSpecificInitializedKey();
            if (!habits.Any())
            {
                var options = new CookieOptions
                {
                    Expires = DateTime.Now.AddMonths(1),
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict
                };
                Response.Cookies.Append(initializedKey, "true", options);
            }

            _logger.LogInformation($"Habit deleted: {id}");
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error deleting habit {id}");
            return StatusCode(500, "Error deleting habit");
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult CompleteHabit(string id)
    {
        try
        {
            var habits = GetHabitsFromCookie();
            var habit = habits.FirstOrDefault(h => h.Id == id);

            if (habit == null)
                return NotFound();

            if (habit.IsCompletedToday())
                return BadRequest("Habit already completed for this period");

            habit.TimesComplete++;
            habit.LastCompletedAt = DateTime.Now;
            SaveHabitsToCookie(habits);

            _logger.LogInformation($"Habit completed: {id}");
            return Json(new { 
                success = true, 
                timesComplete = habit.TimesComplete,
                lastCompletedAt = habit.LastCompletedAt?.ToString("MMM dd, yyyy")
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error completing habit {id}");
            return StatusCode(500, "Error completing habit");
        }
    }

    private async Task<string> SaveImageFile(IFormFile file)
    {
        var userUploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, GetUserSpecificUploadPath());
        Directory.CreateDirectory(userUploadsFolder);

        var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
        var filePath = Path.Combine(userUploadsFolder, uniqueFileName);

        using (var fileStream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(fileStream);
        }

        return $"/{GetUserSpecificUploadPath()}/{uniqueFileName}";
    }

    private List<string> GetCachedImages()
    {
        const string CacheKey = "HabitImages";
        
        if (!_cache.TryGetValue(CacheKey, out List<string> images))
        {
            images = new List<string>
            {
                "/HabitImg/brush.jpg",
                "/HabitImg/food.png",
                "/HabitImg/book.png",
                "/HabitImg/bed.png"
            };

            var cacheOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromHours(24));

            _cache.Set(CacheKey, images, cacheOptions);
        }

        return images;
    }

    private List<HabitViewModel> GetHabitsFromCookie()
    {
        var cookieKey = GetUserSpecificCookieKey();
        var habitsJson = Request.Cookies[cookieKey];
        if (string.IsNullOrEmpty(habitsJson))
            return new List<HabitViewModel>();
            
        return JsonSerializer.Deserialize<List<HabitViewModel>>(habitsJson) ?? new List<HabitViewModel>();
    }

    private void SaveHabitsToCookie(List<HabitViewModel> habits)
    {
        var options = new CookieOptions
        {
            Expires = DateTime.Now.AddMonths(1),
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict
        };
        
        var cookieKey = GetUserSpecificCookieKey();
        Response.Cookies.Append(cookieKey, JsonSerializer.Serialize(habits), options);
    }

    
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId != null)
        {
            Response.Cookies.Delete(GetUserSpecificCookieKey());
            Response.Cookies.Delete(GetUserSpecificInitializedKey());
        }
        
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        HttpContext.Session.Clear();
        return RedirectToAction("Index", "Home");
    }
    
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}