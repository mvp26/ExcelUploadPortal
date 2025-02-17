
namespace ExcelUploadPortal.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Customer
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Salutation is required.")]
    public string Salutation { get; set; }

    [Required(ErrorMessage = "Card Number is required.")]
    [StringLength(16, MinimumLength = 16, ErrorMessage = "Card Number must be 16 digits.")]
    [RegularExpression(@"^\d{16}$", ErrorMessage = "Card Number must contain only numbers.")]
    public string CardNumber { get; set; }

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid Email format.")]

    public string Email { get; set; }

    [Required(ErrorMessage = "Mobile Number is required.")]
    [RegularExpression(@"^\+254\d{9}$", ErrorMessage = "Mobile Number must start with +254 and contain exactly 9 digits.")]
    [Column(TypeName = "VARCHAR(15)")]
    public string MobileNumber { get; set; }

      public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
