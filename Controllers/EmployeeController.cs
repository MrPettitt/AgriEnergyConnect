using AgriEnergyConnect.Data;
using AgriEnergyConnect.Models;
using AgriEnergyConnect.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AgriEnergyConnect.Controllers;

[Authorize(Roles = "Employee")]
public class EmployeeController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly ILogger<EmployeeController> _logger;

    public EmployeeController(
        ApplicationDbContext context,
        UserManager<IdentityUser> userManager,
        ILogger<EmployeeController> logger)
    {
        _context = context;
        _userManager = userManager;
        _logger = logger;
    }

    // GET: Employee/Index
    public async Task<IActionResult> Index()
    {
        return View(new EmployeeDashboardViewModel
        {
            FarmerCount = await _context.Farmers.CountAsync(),
            ProductCount = await _context.Products.CountAsync(),
        });
    }

    // GET: Employee/Farmers
    public async Task<IActionResult> Farmers(int page = 1)
    {
        var query = _context.Farmers
            .AsNoTracking()
            .OrderBy(f => f.FullName)
            .Select(f => new FarmerListItem
            {
                FarmerId = f.FarmerId,
                FullName = f.FullName,
                Email = f.User != null ? f.User.Email ?? string.Empty : string.Empty,
                ContactNumber = f.ContactNumber,
                Address = f.Address,
                ProductCount = f.Products.Count,
            });

        return View(await PagedResult<FarmerListItem>.CreateAsync(query, page));
    }

    // GET: Employee/AddFarmer
    public IActionResult AddFarmer() => View();

    // POST: Employee/AddFarmer
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddFarmer(FarmerRegistrationViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // Accounts created here skip email confirmation; see Program.cs for the reasoning.
        var user = new IdentityUser { UserName = model.Email, Email = model.Email, EmailConfirmed = true };
        var result = await _userManager.CreateAsync(user, model.Password);

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        await _userManager.AddToRoleAsync(user, "Farmer");

        var farmer = new Farmer
        {
            FullName = model.FullName.Trim(),
            ContactNumber = model.ContactNumber,
            Address = model.Address.Trim(),
            UserId = user.Id,
        };

        _context.Farmers.Add(farmer);

        try
        {
            await _context.SaveChangesAsync();
            TempData["Message"] = $"{farmer.FullName} was added as a farmer.";
            return RedirectToAction(nameof(Farmers));
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Failed to save farmer profile for {Email}", model.Email);

            // Do not leave an orphaned login behind if the profile could not be saved.
            await _userManager.DeleteAsync(user);
            ModelState.AddModelError(string.Empty, "Something went wrong while saving the farmer profile. Please try again.");
            return View(model);
        }
    }

    // POST: Employee/DeleteFarmer/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteFarmer(int id)
    {
        var farmer = await _context.Farmers.Include(f => f.User).FirstOrDefaultAsync(f => f.FarmerId == id);
        if (farmer is null)
        {
            return NotFound();
        }

        var name = farmer.FullName;
        var user = farmer.User;

        // Products are removed with the farmer (cascade), then the login itself.
        _context.Farmers.Remove(farmer);
        await _context.SaveChangesAsync();

        if (user is not null)
        {
            await _userManager.DeleteAsync(user);
        }

        TempData["Message"] = $"{name} and their listings were removed.";
        return RedirectToAction(nameof(Farmers));
    }

    // GET: Employee/ViewProducts
    public async Task<IActionResult> ViewProducts([FromQuery] ProductFilterViewModel filter)
    {
        var query = _context.Products.AsNoTracking().Include(p => p.Farmer).AsQueryable();

        if (filter.FarmerId.HasValue)
        {
            query = query.Where(p => p.FarmerId == filter.FarmerId);
        }

        if (!string.IsNullOrWhiteSpace(filter.Category))
        {
            query = query.Where(p => p.ProductCategory == filter.Category);
        }

        if (filter.StartDate.HasValue)
        {
            var start = filter.StartDate.Value.Date;
            query = query.Where(p => p.ProductionDate >= start);
        }

        if (filter.EndDate.HasValue)
        {
            var end = filter.EndDate.Value.Date;
            query = query.Where(p => p.ProductionDate <= end);
        }

        var ordered = query.OrderByDescending(p => p.ProductionDate).ThenBy(p => p.ProductName);

        filter.Farmers = await BuildFarmerSelectListAsync(filter.FarmerId);
        filter.Results = await PagedResult<Product>.CreateAsync(ordered, filter.Page);
        filter.Page = filter.Results.Page;
        return View(filter);
    }

    private async Task<SelectList> BuildFarmerSelectListAsync(int? selectedFarmerId)
    {
        var farmers = await _context.Farmers.AsNoTracking().OrderBy(f => f.FullName).ToListAsync();
        return new SelectList(farmers, nameof(Farmer.FarmerId), nameof(Farmer.FullName), selectedFarmerId);
    }
}
