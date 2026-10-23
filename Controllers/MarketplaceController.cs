using AgriEnergyConnect.Data;
using AgriEnergyConnect.Models;
using AgriEnergyConnect.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AgriEnergyConnect.Controllers;

/// <summary>
/// A public, read-only catalog of every farmer's produce. No login required,
/// unlike the Farmer and Employee dashboards.
/// </summary>
public class MarketplaceController : Controller
{
    private readonly ApplicationDbContext _context;

    public MarketplaceController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: Marketplace
    public async Task<IActionResult> Index(string? search, string? category, int page = 1)
    {
        var query = _context.Products.AsNoTracking().Include(p => p.Farmer).AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            // LIKE is case-insensitive on SQLite; Contains() would translate to a case-sensitive instr().
            var escaped = search.Trim().Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_");
            query = query.Where(p => EF.Functions.Like(p.ProductName, $"%{escaped}%", "\\"));
        }

        if (!string.IsNullOrWhiteSpace(category))
        {
            query = query.Where(p => p.ProductCategory == category);
        }

        var ordered = query.OrderByDescending(p => p.ProductionDate).ThenBy(p => p.ProductName);

        var categories = await _context.Products
            .AsNoTracking()
            .Select(p => p.ProductCategory)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync();

        return View(new MarketplaceViewModel
        {
            Search = search,
            Category = category,
            Categories = categories,
            Results = await PagedResult<Product>.CreateAsync(ordered, page),
        });
    }

    // GET: Marketplace/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var product = await _context.Products
            .AsNoTracking()
            .Include(p => p.Farmer)
            .FirstOrDefaultAsync(p => p.ProductId == id);

        if (product?.Farmer is null)
        {
            return NotFound();
        }

        var more = await _context.Products
            .AsNoTracking()
            .Where(p => p.FarmerId == product.FarmerId && p.ProductId != product.ProductId)
            .OrderByDescending(p => p.ProductionDate)
            .Take(4)
            .ToListAsync();

        return View(new ProductDetailsViewModel { Product = product, MoreFromFarmer = more });
    }
}
