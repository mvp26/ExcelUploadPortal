//using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ExcelUploadPortal.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace ExcelUploadPortal.Data
{
    public class ExcelUploadPortalContext : IdentityDbContext<IdentityUser>
    {
        public ExcelUploadPortalContext(DbContextOptions<ExcelUploadPortalContext> options)
            : base(options)  // Pass the DbContextOptions to the base class constructor
        {
        }

        public DbSet<Customer> Customers { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // Customize the ASP.NET Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Identity table names and more.
            // Add your customizations after calling base.OnModelCreating(builder);
        }
    }
}
