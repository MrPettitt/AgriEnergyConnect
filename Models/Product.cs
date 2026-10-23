namespace AgriEnergyConnect.Models;

/// <summary>
/// A produce listing submitted by a farmer.
/// </summary>
public class Product
{
    public int ProductId { get; set; }

    public int FarmerId { get; set; }

    public string ProductName { get; set; } = string.Empty;

    public string ProductCategory { get; set; } = string.Empty;

    /// <summary>The unit the price applies to, e.g. "kg" or "dozen".</summary>
    public string Unit { get; set; } = "kg";

    public DateTime ProductionDate { get; set; } = DateTime.Today;

    public decimal Price { get; set; }

    public Farmer? Farmer { get; set; }
}
