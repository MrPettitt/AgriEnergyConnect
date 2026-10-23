using Microsoft.AspNetCore.Identity;

namespace AgriEnergyConnect.Models;

/// <summary>
/// A farmer profile linked one-to-one with an Identity user account.
/// Created by an employee via the "Add Farmer" workflow.
/// </summary>
public class Farmer
{
    public int FarmerId { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string ContactNumber { get; set; } = string.Empty;

    public string Address { get; set; } = string.Empty;

    public string UserId { get; set; } = string.Empty;

    public IdentityUser? User { get; set; }

    public ICollection<Product> Products { get; set; } = new List<Product>();
}
