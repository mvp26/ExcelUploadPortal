//using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ExcelUploadPortal.Models;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace ExcelUploadPortal.Data
{
    public class ExcelUploadPortalContext:DbContext 
    {
        public ExcelUploadPortalContext(DbContextOptions<ExcelUploadPortalContext> options)
        : base(options)
        {
        }

        public DbSet<Customer> Customers { get; set; }

        
    }
}
