using System.Diagnostics;
using AgriEnergyConnect.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace AgriEnergyConnect.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        // Signed-in users land straight on their dashboard instead of the marketing page.
        if (User.Identity?.IsAuthenticated == true)
        {
            if (User.IsInRole("Farmer"))
            {
                return RedirectToAction("Index", "Farmer");
            }

            if (User.IsInRole("Employee"))
            {
                return RedirectToAction("Index", "Employee");
            }
        }

        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
