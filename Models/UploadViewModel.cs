using ExcelUploadPortal.Models;
using System.Collections.Generic;

namespace ExcelUploadPortal.Models
{
    public class UploadViewModel
    {
       
        
            public List<IFormFile> Files { get; set; }
            public List<bool> FileSelection { get; set; } // Track which files are selected
        }


    }

    // You may need a CustomerStatus class for tracking the success/failure of each upload attempt


