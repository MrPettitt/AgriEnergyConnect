using AgriEnergyConnect.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AgriEnergyConnect.ViewModels;

public class MarketplaceViewModel
{
    public string? Search { get; init; }

    public string? Category { get; init; }

    public IReadOnlyList<string> Categories { get; init; } = Array.Empty<string>();

    public required PagedResult<Product> Results { get; init; }
}

public class ProductDetailsViewModel
{
    public required Product Product { get; init; }

    public IReadOnlyList<Product> MoreFromFarmer { get; init; } = Array.Empty<Product>();
}

public class FarmerDashboardViewModel
{
    public string? FarmerName { get; init; }

    public required PagedResult<Product> Products { get; init; }
}

public class EmployeeDashboardViewModel
{
    public int FarmerCount { get; init; }

    public int ProductCount { get; init; }
}

public class FarmerListItem
{
    public int FarmerId { get; init; }

    public string FullName { get; init; } = string.Empty;

    public string Email { get; init; } = string.Empty;

    public string ContactNumber { get; init; } = string.Empty;

    public string Address { get; init; } = string.Empty;

    public int ProductCount { get; init; }
}

public class ProductFilterViewModel
{
    public int? FarmerId { get; set; }

    public string? Category { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public int Page { get; set; } = 1;

    public SelectList Farmers { get; set; } = new(Array.Empty<Farmer>());

    public PagedResult<Product> Results { get; set; } = new();
}
