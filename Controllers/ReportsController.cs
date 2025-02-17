using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using ExcelUploadPortal.Models;
using System;
using System.IO;
using System.Linq;
using ExcelUploadPortal.Data;
using Serilog;
using Microsoft.AspNetCore.Authorization;
[Authorize(Roles = "Admin")]
public class ReportsController : Controller
{
    private readonly ExcelUploadPortalContext _context;

    public ReportsController(ExcelUploadPortalContext context)
    {
        _context = context;
    }

    [HttpGet]
    [Route("Index")]
    public IActionResult Index()
    {
        return View();
    }

    [HttpGet]
    public IActionResult ExportToExcel(DateTime? startDate, DateTime? endDate)
    {
        try
        {
            Log.Information("ExportToExcel started with StartDate: {StartDate}, EndDate: {EndDate}", startDate, endDate);

            var data = _context.Customers.AsQueryable();

            if (startDate.HasValue)
            {
                data = data.Where(d => d.CreatedAt >= startDate.Value);
            }
            if (endDate.HasValue)
            {
                data = data.Where(d => d.CreatedAt <= endDate.Value);
            }

            var filteredData = data.ToList();
            Log.Information("Filtered {Count} records for the report.", filteredData.Count);

            // Set EPPlus License Context
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Customer Report");

                // Set Headers
                var headers = new[] { "Salutation", "Email", "Mobile Number", "Card Number" };
                for (int i = 0; i < headers.Length; i++)
                {
                    worksheet.Cells[1, i + 1].Value = headers[i];
                }

                // Populate Data
                for (int row = 0; row < filteredData.Count; row++)
                {
                    worksheet.Cells[row + 2, 1].Value = filteredData[row].Salutation?.Trim();
                    worksheet.Cells[row + 2, 2].Value = filteredData[row].Email?.Trim();
                    worksheet.Cells[row + 2, 3].Value = filteredData[row].MobileNumber?.Trim();
                    worksheet.Cells[row + 2, 4].Value = filteredData[row].CardNumber?.Trim();
                }

                // Auto-fit columns
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;

                var fileName = $"Customer_Report_{DateTime.UtcNow:yyyyMMdd}.xlsx";

                Log.Information("Excel report generated successfully.");

                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error generating Excel report.");
            return BadRequest("An error occurred while generating the Excel report.");
        }
    }
}
