using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ExcelUploadPortal.Data;
using Pomelo.EntityFrameworkCore.MySql;

var builder = WebApplication.CreateBuilder(args);

// Get the MySQL connection string
var connectionString = builder.Configuration.GetConnectionString("ExcelUploadPortalContextConnection")
    ?? throw new InvalidOperationException("Connection string 'ExcelUploadPortalContextConnection' not found.");

// Register the DbContext with MySQL
builder.Services.AddDbContext<ExcelUploadPortalContext>(options =>
    options.UseMySql(
        connectionString,
        new MySqlServerVersion(new Version(8, 0, 32)) // Replace with your MySQL version
    ));

// Add Identity and configure default options
builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ExcelUploadPortalContext>();

// Add services to the container
builder.Services.AddControllersWithViews();

// Add Razor Pages for Identity
builder.Services.AddRazorPages();  // Ensure Razor Pages are available

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Add authentication and authorization middleware
app.UseAuthentication();
app.UseAuthorization();

// Configure endpoints, including mapping Razor Pages for Identity
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");
    endpoints.MapRazorPages();  // Ensure Razor Pages are mapped for Identity
});

// Map Razor Pages
app.MapRazorPages(); // This line ensures Razor Pages are mapped for Identity views like Login, Register, etc.

app.Run();
