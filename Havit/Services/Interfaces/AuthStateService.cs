using Microsoft.AspNetCore.Http;

namespace Havit.Services.Managers;

public class AuthStateService : IAuthStateService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuthStateService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public bool IsAuthenticated => 
        _httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated ?? false;

    public string? CurrentUsername => 
        _httpContextAccessor.HttpContext?.User.Identity?.Name;

    public string? CurrentUserRole =>
        _httpContextAccessor.HttpContext?.User.Claims
            .FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Role)?.Value;
}

public class AuthStateMiddleware
{
    private readonly RequestDelegate _next;

    public AuthStateMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Path.StartsWithSegments("/api"))
        {
            await _next(context);
            return;
        }

        if (context.Request.Method == "GET" && !context.Request.Headers.Accept.Contains("application/json"))
        {
            var isAuthenticated = context.User.Identity?.IsAuthenticated ?? false;
            context.Items["IsAuthenticated"] = isAuthenticated;
            
            if (isAuthenticated)
            {
                context.Items["CurrentUsername"] = context.User.Identity?.Name;
                context.Items["CurrentUserRole"] = context.User.Claims
                    .FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Role)?.Value;
            }
        }

        await _next(context);
    }
}