using System.Security.Claims;
using AgriEnergyConnect.Data;
using AgriEnergyConnect.Models;
using AgriEnergyConnect.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AgriEnergyConnect.Controllers;

[Authorize(Roles = "Farmer")]
public class FarmerController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<FarmerController> _logger;

    public FarmerController(ApplicationDbContext context, ILogger<FarmerController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // GET: Farmer/Index
    public async Task<IActionResult> Index(int page = 1)
    {
        var farmer = await GetCurrentFarmerAsync();
        var farmerId = farmer?.FarmerId ?? -1;

        var query = _context.Products
            .AsNoTracking()
            .Where(p => p.FarmerId == farmerId)
            .OrderByDescending(p => p.ProductionDate)
            .ThenBy(p => p.ProductName);

        return View(new FarmerDashboardViewModel
        {
            FarmerName = farmer?.FullName,
            Products = await PagedResult<Product>.CreateAsync(query, page),
        });
    }

    // GET: Farmer/AddProduct
    public IActionResult AddProduct() => View(new ProductFormViewModel());

    // POST: Farmer/AddProduct
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddProduct(ProductFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var farmer = await GetCurrentFarmerAsync();
        if (farmer is null)
        {
            ModelState.AddModelError(string.Empty, "We could not find a farmer profile linked to your account.");
            return View(model);
        }

        var product = new Product { FarmerId = farmer.FarmerId };
        Apply(model, product);
        _context.Products.Add(product);

        try
        {
            await _context.SaveChangesAsync();
            TempData["Message"] = $"{product.ProductName} was added successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Failed to save product {ProductName} for farmer {FarmerId}", product.ProductName, farmer.FarmerId);
            ModelState.AddModelError(string.Empty, "Something went wrong while saving the product. Please try again.");
            return View(model);
        }
    }

    // GET: Farmer/EditProduct/5
    public async Task<IActionResult> EditProduct(int id)
    {
        var product = await FindOwnProductAsync(id);
        if (product is null)
        {
            return NotFound();
        }

        ViewData["ProductId"] = id;
        return View(new ProductFormViewModel
        {
            ProductName = product.ProductName,
            ProductCategory = product.ProductCategory,
            Unit = product.Unit,
            ProductionDate = product.ProductionDate,
            Price = product.Price,
        });
    }

    // POST: Farmer/EditProduct/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditProduct(int id, ProductFormViewModel model)
    {
        var product = await FindOwnProductAsync(id);
        if (product is null)
        {
            return NotFound();
        }

        ViewData["ProductId"] = id;

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        Apply(model, product);

        try
        {
            await _context.SaveChangesAsync();
            TempData["Message"] = $"{product.ProductName} was updated.";
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Failed to update product {ProductId}", id);
            ModelState.AddModelError(string.Empty, "Something went wrong while saving the product. Please try again.");
            return View(model);
        }
    }

    // POST: Farmer/DeleteProduct/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var product = await FindOwnProductAsync(id);
        if (product is null)
        {
            return NotFound();
        }

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();
        TempData["Message"] = $"{product.ProductName} was removed.";
        return RedirectToAction(nameof(Index));
    }

    private static void Apply(ProductFormViewModel model, Product product)
    {
        product.ProductName = model.ProductName.Trim();
        product.ProductCategory = model.ProductCategory;
        product.Unit = model.Unit;
        product.ProductionDate = model.ProductionDate.Date;
        product.Price = model.Price;
    }

    private Task<Farmer?> GetCurrentFarmerAsync()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return _context.Farmers.FirstOrDefaultAsync(f => f.UserId == userId);
    }

    /// <summary>Loads a product only if it belongs to the signed-in farmer.</summary>
    private async Task<Product?> FindOwnProductAsync(int id)
    {
        var farmer = await GetCurrentFarmerAsync();
        if (farmer is null)
        {
            return null;
        }

        return await _context.Products.FirstOrDefaultAsync(p => p.ProductId == id && p.FarmerId == farmer.FarmerId);
    }
}
