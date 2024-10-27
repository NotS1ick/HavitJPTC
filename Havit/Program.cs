using Havit.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

// Determine if we're running in Azure Static Web Apps
bool isStaticWebApp = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("WEBSITE_INSTANCE_ID"));

// Configure services based on environment
if (isStaticWebApp)
{
    // For Static Web Apps, use Azure Storage or Cosmos DB
    // You'll need to add the appropriate connection string in your Static Web Apps configuration
    var staticWebAppsConnectionString = Environment.GetEnvironmentVariable("AZURE_STORAGE_CONNECTION_STRING");
    
    // TODO: Replace with your chosen storage solution
    // Example with Azure Storage:
    // builder.Services.AddSingleton<IFileProvider>(new AzureStorageFileProvider(staticWebAppsConnectionString));
}
else
{
    // Local development - use SQL Server
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlServer(connectionString));
}

// Add MVC services
builder.Services.AddControllersWithViews();

// Configure Identity
builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
{
    options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
    options.User.RequireUniqueEmail = false;
    
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 8;
    options.Password.RequiredUniqueChars = 1;
    
    // Add cookie settings for Static Web Apps
    options.Cookies.ApplicationCookie.Cookie.SameSite = SameSiteMode.Strict;
    options.Cookies.ApplicationCookie.Cookie.SecurePolicy = CookieSecurePolicy.Always;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// Add Static Web Apps specific services
builder.Services.AddAzureStaticWebApps();

var app = builder.Build();

// Database migrations only for local development
if (!isStaticWebApp)
{
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        try
        {
            var context = services.GetRequiredService<AppDbContext>();
            context.Database.Migrate();
        }
        catch (Exception ex)
        {
            var logger = services.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "An error occurred while migrating the database.");
        }
    }
}

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

// Configure static files with cache control
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        // Cache static files for 1 year
        ctx.Context.Response.Headers.Append(
            "Cache-Control", "public,max-age=31536000");
    }
});

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// Add Static Web Apps middleware
app.UseStaticWebApps();

// Configure routes
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Handle SPA fallback for Static Web Apps
app.MapFallbackToFile("index.html");

app.Run();
