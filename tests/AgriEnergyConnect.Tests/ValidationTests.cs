using System.ComponentModel.DataAnnotations;
using AgriEnergyConnect.ViewModels;
using Xunit;

namespace AgriEnergyConnect.Tests;

public class ValidationTests
{
    private static ProductFormViewModel Valid() => new()
    {
        ProductName = "Tomatoes",
        ProductCategory = "Vegetable",
        Unit = "kg",
        ProductionDate = DateTime.Today,
        Price = 2.5m,
    };

    private static IList<ValidationResult> Validate(object model)
    {
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(model, new ValidationContext(model), results, validateAllProperties: true);
        return results;
    }

    [Fact]
    public void ValidProduct_HasNoErrors() => Assert.Empty(Validate(Valid()));

    [Fact]
    public void FutureProductionDate_IsRejected()
    {
        var model = Valid();
        model.ProductionDate = DateTime.Today.AddDays(1);
        Assert.Contains(Validate(model), r => r.MemberNames.Contains(nameof(ProductFormViewModel.ProductionDate)));
    }

    [Fact]
    public void UnknownCategory_IsRejected()
    {
        var model = Valid();
        model.ProductCategory = "Veggies";
        Assert.Contains(Validate(model), r => r.MemberNames.Contains(nameof(ProductFormViewModel.ProductCategory)));
    }

    [Fact]
    public void UnknownUnit_IsRejected()
    {
        var model = Valid();
        model.Unit = "furlong";
        Assert.Contains(Validate(model), r => r.MemberNames.Contains(nameof(ProductFormViewModel.Unit)));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(100001)]
    public void OutOfRangePrice_IsRejected(double price)
    {
        var model = Valid();
        model.Price = (decimal)price;
        Assert.Contains(Validate(model), r => r.MemberNames.Contains(nameof(ProductFormViewModel.Price)));
    }

    [Fact]
    public void FarmerRegistration_RejectsShortPassword()
    {
        var model = new FarmerRegistrationViewModel
        {
            FullName = "Jane Smith",
            ContactNumber = "0821234567",
            Address = "Farm Road",
            Email = "jane@example.com",
            Password = "short",
        };
        Assert.Contains(Validate(model), r => r.MemberNames.Contains(nameof(FarmerRegistrationViewModel.Password)));
    }
}
