using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using ExcelUploadPortal.Data;
using ExcelUploadPortal.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Serilog;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

[Authorize(Roles = "Admin")]
[Route("Home")]
public class FilesController : Controller
{
    private readonly ExcelUploadPortalContext _context;
    //private readonly SignInManager<IdentityUser> _signInManager;
    private readonly ILogger<FilesController> _logger;

    // Path for storing uploaded files
    private readonly string _uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "UploadedFiles");

    public FilesController(ExcelUploadPortalContext context, ILogger<FilesController> logger)
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        _context = context;
        //_signInManager = signInManager;
        _logger = logger;

        // Ensure upload folder exists
        if (!Directory.Exists(_uploadFolder))
        {
            Directory.CreateDirectory(_uploadFolder);
        }
    }

    [HttpGet]
    [Route("Index")]
    public IActionResult Index()
    {
        return View();
    }



    [HttpPost]
    [Route("UploadExcel")]
    public async Task<IActionResult> UploadExcel(IEnumerable<IFormFile> files)
    {
        var fileCustomerMap = new Dictionary<string, List<Customer>>();
        var fileErrors = new List<string>();

        if (files == null)
        {
            TempData["Error"] = "Please upload at least one Excel file.";
            _logger.LogWarning("No files uploaded.");
            return RedirectToAction(nameof(Index));
        }

        // Validate uploaded files
        var allowedMimeTypes = new[] {
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "application/vnd.ms-excel"
        };

        var uploadedFilePaths = new List<string>();

        foreach (var file in files)
        {
            try
            {
                if (!allowedMimeTypes.Contains(file.ContentType))
                {
                    fileErrors.Add($"Unsupported file format for {file.FileName}. Please upload an Excel file.");
                    continue;
                }

                var filePath = Path.Combine(_uploadFolder, Path.GetFileName(file.FileName));
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }

                uploadedFilePaths.Add(filePath);

                // Process the file
                using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                using (var package = new ExcelPackage(stream))
                {
                    var worksheet = package.Workbook.Worksheets.FirstOrDefault();
                    if (worksheet == null)
                    {
                        fileErrors.Add($"The uploaded Excel file {file.FileName} is empty or invalid.");
                        continue;
                    }

                    if (!ValidateHeaderRow(worksheet, fileErrors, file.FileName))
                    {
                        continue;
                    }

                    var customers = new List<Customer>();
                    int rowCount = worksheet.Dimension.Rows;

                    for (int row = 2; row <= rowCount; row++)
                    {
                        var customer = ProcessRow(worksheet, row, file.FileName, fileErrors);
                        if (customer != null) customers.Add(customer);
                    }

                    fileCustomerMap[file.FileName] = customers;
                }
            }
            catch (Exception ex)
            {
                fileErrors.Add($"Error processing file {file.FileName}: {ex.Message}");
                _logger.LogError(ex, $"Error processing file {file.FileName}");
            }
        }

        // Save records to the database if no errors occurred
        if (fileCustomerMap.Any() && !fileErrors.Any())
        {
            try
            {
                foreach (var fileRecords in fileCustomerMap.Values)
                {
                    _context.Customers.AddRange(fileRecords);
                }

                await _context.SaveChangesAsync();
                TempData["Success"] = GenerateSavedRecordsHtmlWithFiles(fileCustomerMap, uploadedFilePaths);
            }
            catch (Exception ex)
            {
                fileErrors.Add($"Error saving records to the database: {ex.Message}");
                _logger.LogError(ex, "Error saving records to the database.");
            }
        }
        else
        {
            TempData["Error"] = $"Some files had errors: {string.Join(", ", fileErrors)}";
            _logger.LogWarning($"File errors occurred: {string.Join(", ", fileErrors)}");
        }

        return RedirectToAction(nameof(Index));
    }

    private Customer ProcessRow(ExcelWorksheet worksheet, int row, string fileName, List<string> fileErrors)
{
    try
    {
        string salutation = worksheet.Cells[row, 1].Text.Trim();
        string email = worksheet.Cells[row, 2].Text.Trim();
        string mobileNumber = worksheet.Cells[row, 3].Text.Trim();
        string cardNumber = worksheet.Cells[row, 4].Text.Trim();

        var validationErrors = new List<string>();

        // Validate Salutation
        if (string.IsNullOrWhiteSpace(salutation))
            validationErrors.Add("Salutation is required.");

        // Validate Email
        if (string.IsNullOrWhiteSpace(email) || !new EmailAddressAttribute().IsValid(email))
            validationErrors.Add("Invalid email format.");

            // Validate Mobile Number (10 digits)@"^\+254\d{9}$"
            if (string.IsNullOrWhiteSpace(mobileNumber) || !Regex.IsMatch(mobileNumber, @"^\+254\d{9}$"))
            validationErrors.Add("Mobile Number must be between 10 and contain only numbers.");

        // Validate Card Number (exactly 16 digits)
        if (string.IsNullOrWhiteSpace(cardNumber) || !Regex.IsMatch(cardNumber, @"^\d{16}$"))
            validationErrors.Add("Card Number must be exactly 16 digits and contain only numbers.");

        if (validationErrors.Any())
        {
            fileErrors.Add($"Row {row} in file {fileName} has errors: {string.Join("; ", validationErrors)}");
            return null;
        }

        return new Customer
        {
            Salutation = salutation,
            Email = email,
            MobileNumber = mobileNumber,
            CardNumber = cardNumber
            
        };
    }
    catch (Exception ex)
    {
        fileErrors.Add($"Error processing row {row} in file {fileName}: {ex.Message}");
        _logger.LogError(ex, $"Error processing row {row}");
        return null;
    }
}

    private bool ValidateHeaderRow(ExcelWorksheet worksheet, List<string> fileErrors, string fileName)
    {
        var requiredHeaders = new[] { "Salutation", "Email", "MobileNumber", "CardNumber" };

        var headers = new List<string>();
        for (int col = 1; col <= worksheet.Dimension.Columns; col++)
        {
            headers.Add(worksheet.Cells[1, col].Text.Trim());
        }

        var missingHeaders = requiredHeaders.Except(headers).ToList();
        if (missingHeaders.Any())
        {
            fileErrors.Add($"File {fileName} is missing the following required headers: {string.Join(", ", missingHeaders)}");
            return false;
        }

        return true;
    }

    private string GenerateSavedRecordsHtmlWithFiles(Dictionary<string, List<Customer>> fileCustomerMap, List<string> uploadedFilePaths)
    {
        var htmlBuilder = new System.Text.StringBuilder();

        foreach (var file in fileCustomerMap)
        {
            htmlBuilder.AppendLine($"<h3>Records from File: {file.Key}</h3>");
            htmlBuilder.AppendLine("<table border='1' style='border-collapse: collapse; width: 90%; margin: 20px auto; font-family: Arial, sans-serif; font-size: 16px;'>");
            htmlBuilder.AppendLine("<thead style='background-color: #4CAF50; color: white;'>");
            htmlBuilder.AppendLine("<tr><th>Salutation</th><th>Email</th><th>Mobile Number</th><th>Card Number</th></tr>");
            htmlBuilder.AppendLine("</thead><tbody>");

            foreach (var customer in file.Value)
            {
                htmlBuilder.AppendLine($"<tr><td>{customer.Salutation}</td><td>{customer.Email}</td><td>{customer.MobileNumber}</td><td>{customer.CardNumber}</td></tr>");
            }

            htmlBuilder.AppendLine("</tbody></table>");
        }

        var fileLinks = "<div>" +
                         "<h3>Uploaded Files</h3>" +
                         string.Join("", uploadedFilePaths.Select(filePath =>
                             $"<a href='/Home/DownloadFile/{Path.GetFileName(filePath)}' download>{Path.GetFileName(filePath)}</a><br>")) +
                         "</div>";

        return htmlBuilder.ToString() + fileLinks;
    }

    [HttpGet]
    [Route("DownloadFile/{fileName}")]
        public IActionResult DownloadFile(string fileName)
    {
            var filePath = Path.Combine(_uploadFolder, fileName);

            if (!System.IO.File.Exists(filePath))
            {
                TempData["Error"] = "The requested file does not exist.";
                _logger.LogWarning($"File not found: {filePath}");
                return RedirectToAction(nameof(Index));
            }

            var contentType = "application/octet-stream";
            return PhysicalFile(filePath, contentType, fileName);



        }
        
    }
