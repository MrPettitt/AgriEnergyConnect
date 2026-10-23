using Microsoft.AspNetCore.Identity;

namespace AgriEnergyConnect.Models;

/// <summary>
/// An employee profile linked one-to-one with an Identity user account.
/// Employees manage farmer profiles and review submitted produce.
/// </summary>
public class Employee
{
    public int EmployeeId { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string ContactNumber { get; set; } = string.Empty;

    public string Position { get; set; } = string.Empty;

    public string UserId { get; set; } = string.Empty;

    public IdentityUser? User { get; set; }
}
