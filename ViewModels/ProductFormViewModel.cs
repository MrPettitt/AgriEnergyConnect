using System.ComponentModel.DataAnnotations;
using AgriEnergyConnect.Models;
using AgriEnergyConnect.Validation;

namespace AgriEnergyConnect.ViewModels;

/// <summary>
/// The fields a farmer can set when adding or editing a product. Kept separate from
/// <see cref="Product"/> so ids and ownership can never be set from the form.
/// </summary>
public class ProductFormViewModel
{
    [Required(ErrorMessage = "Please enter a product name.")]
    [StringLength(100)]
    [Display(Name = "Product Name")]
    public string ProductName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please choose a product category.")]
    [OneOf(typeof(ProductOptions), nameof(ProductOptions.Categories), ErrorMessage = "Please choose a category from the list.")]
    [Display(Name = "Product Category")]
    public string ProductCategory { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please choose a unit.")]
    [OneOf(typeof(ProductOptions), nameof(ProductOptions.Units), ErrorMessage = "Please choose a unit from the list.")]
    [Display(Name = "Sold per")]
    public string Unit { get; set; } = "kg";

    [Required(ErrorMessage = "Please enter a production date.")]
    [DataType(DataType.Date)]
    [NotInFuture(ErrorMessage = "The production date cannot be in the future.")]
    [Display(Name = "Production Date")]
    public DateTime ProductionDate { get; set; } = DateTime.Today;

    [Required(ErrorMessage = "Please enter a price.")]
    [Range(0.01, 100000, ErrorMessage = "Please enter a price greater than 0.")]
    [DataType(DataType.Currency)]
    [Display(Name = "Price")]
    public decimal Price { get; set; }
}
