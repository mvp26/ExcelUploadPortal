namespace ExcelUploadPortal.Models
{
    public class Customer
    {
        public object UploadedFileName;

        public int Id { get; set; }
        public string Salutation { get; set; }
        public int CardNumber { get; set; }
        public string Email { get; set; }
        public string MobileNumber { get; set; }
    }
}
