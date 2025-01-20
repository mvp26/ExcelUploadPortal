using ExcelUploadPortal.Models;

namespace ExcelUploadPortal.Controllers
{
    public class CustomerStatus
    {
        public Customer Customer { get; set; }
        public bool IsSuccessful { get; set; }
    }
}