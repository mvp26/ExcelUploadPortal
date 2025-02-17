using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.Identity.Web.UI;
using ExcelUploadPortal.Data;
using Pomelo.EntityFrameworkCore.MySql;
using Serilog;
using Rotativa.AspNetCore;
using Microsoft.Extensions.Hosting;
using ExcelUploadPortal.Services;
using ExcelUploadPortal.Repository;
using Microsoft.AspNetCore.Authentication.Cookies;
var builder = WebApplication.CreateBuilder(args);

// Configure Serilog logging
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/log.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
});
builder.Services.AddScoped<RedisCacheService>();

// Register Repository
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();

try
{
    Log.Information("Starting up the application...");

    var connectionString = builder.Configuration.GetConnectionString("ExcelUploadPortalContextConnection")
        ?? throw new InvalidOperationException("Connection string 'ExcelUploadPortalContextConnection' not found.");

    // Register DbContext with MySQL
    builder.Services.AddDbContext<ExcelUploadPortalContext>(options =>
        options.UseMySql(
            connectionString,
            new MySqlServerVersion(new Version(8, 0, 32))
        ));

    // Add LDAP Authentication
    //builder.Services.AddAuthentication(NegotiateDefaults.AuthenticationScheme)
    //    .AddNegotiate();

    //builder.Services.AddAuthorization(options =>
    //{
    //    options.FallbackPolicy = options.DefaultPolicy;
    //});

    // Register MVC controllers and Razor Pages
    builder.Services.AddControllersWithViews();
    builder.Services.AddRazorPages();
    builder.Services.AddScoped<LdapAuthenticationService>();

    var app = builder.Build();

    // Configure error handling
    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Home/Error");
        app.UseHsts();
    }

    app.UseHttpsRedirection();
    app.UseStaticFiles();
    app.UseRouting();

    // Enable authentication & authorization middleware
    app.UseAuthentication();
    app.UseAuthorization();

    // Configure endpoints, including Identity Razor Pages
    app.UseEndpoints(endpoints =>
    {
        endpoints.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");
        endpoints.MapRazorPages();
    });

    Log.Information("Application started successfully.");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application startup failed.");
}
finally
{
    Log.CloseAndFlush();
}
