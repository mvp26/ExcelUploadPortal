using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Serilog;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ExcelUploadPortal.Data;
using ExcelUploadPortal.Models;
using Microsoft.EntityFrameworkCore;
using ExcelUploadPortal.Services;
using ExcelUploadPortal.Repository;
using DocumentFormat.OpenXml.Office2010.Excel;
using Microsoft.AspNetCore.Authorization;
[Authorize(Roles = "Admin")]
[Route("Customer")]
public class CustomerController : Controller
{
    private readonly ExcelUploadPortalContext _context;
    private readonly RedisCacheService _cacheService;
    private readonly ICustomerRepository _customerRepository;
    public CustomerController(ExcelUploadPortalContext context, RedisCacheService cacheService,ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
        _context = context;
        _cacheService = cacheService;
    }

    // GET: Customers with Caching
    [HttpGet("Index")]
    public async Task<IActionResult> Index()
    {
        try
        {
            const string cacheKey = "CustomerList";
            var customers = await _cacheService.GetCacheAsync<List<Customer>>(cacheKey);

            if (customers == null) // Cache miss, fetch from DB
            {
                Log.Information("Fetching customers from database...");
                customers = await _customerRepository.GetAllCustomersAsync();
                // Store result in Redis with a 10-minute expiration
                await _cacheService.SetCacheAsync(cacheKey, customers, TimeSpan.FromMinutes(10));
            }
            else
            {
                Log.Information("Fetching customers from Redis cache...");
            }

            return View(customers);
        }
        catch (Exception ex)
        {
            Log.Error($"Error fetching customers: {ex.Message}");
            return StatusCode(500, "An error occurred.");
        }
    }

    // Clear Cache on Create, Edit, or Delete
    private async Task ClearCache()
    {
        await _cacheService.RemoveCacheAsync("CustomerList");
    }

    // GET: Show Create Customer Form
    [HttpGet("Create")]
    public IActionResult Create()
    {
        return View(); 
    }

    // POST: Create Customer (Invalidate Cache)
    [HttpPost("Create")]
    [ValidateAntiForgeryToken] // Prevent CSRF attacks
    public async Task<IActionResult> Create(Customer customer)
    {
        if (!ModelState.IsValid) return View(customer);

        try
        {
            await _customerRepository.AddCustomerAsync(customer);
            await ClearCache(); // Invalidate cache
            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            Log.Error($"Error creating customer: {ex.Message}");
            return StatusCode(500, "An error occurred.");
        }
    }

    // GET: Show Edit Customer Form
    [HttpGet("Edit")]
    public async Task<IActionResult> Edit(int id)
    {
        var customer = await _customerRepository.GetCustomerByIdAsync(id);
        if (customer == null) return NotFound();

        return View(customer);
    }

    // POST: Edit Customer (Invalidate Cache)
    [HttpPost("Edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Customer customer)
    {
        if (!ModelState.IsValid) return View(customer);

        try
        {
            await _customerRepository.UpdateCustomerAsync(customer);
            await ClearCache();
            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            Log.Error($"Error updating customer: {ex.Message}");
            return StatusCode(500, "An error occurred.");
        }
    }


    //// GET: Show Delete Confirmation Page
    [HttpGet("Delete")]
    public async Task<IActionResult> Delete(int id)
    {
        var customer = await _customerRepository.GetCustomerByIdAsync(id);
        if (customer == null) return NotFound();

        return View(customer);
    }

    // POST: Delete Customer (Invalidate Cache)
    [HttpPost("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Customer customer)
    {
        try
        {
            await _customerRepository.DeleteCustomerAsync(customer);
            await ClearCache();
            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            Log.Error($"Error deleting customer: {ex.Message}");
            return StatusCode(500, "An error occurred.");
        }
    }
}
