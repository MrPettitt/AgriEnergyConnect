namespace AgriEnergyConnect.Models;

/// <summary>
/// The fixed lists of categories and units a product can use, so listings
/// stay consistent ("Vegetable", not "veg" or "Veggies").
/// </summary>
public static class ProductOptions
{
    public static readonly IReadOnlyList<string> Categories = new[]
    {
        "Fruit", "Vegetable", "Grain", "Dairy", "Eggs", "Meat", "Herbs", "Other",
    };

    public static readonly IReadOnlyList<string> Units = new[]
    {
        "kg", "g", "each", "dozen", "litre", "bunch",
    };
}
