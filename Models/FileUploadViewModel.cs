namespace ExcelUploadPortal.Models
{
    public class FileUploadViewModel
    {
        public List<string> FileDownloadLinks { get; set; }
        public List<Customer> PreviewData { get; set; }
        public List<IFormFile> Files { get; set; }
    }
}
