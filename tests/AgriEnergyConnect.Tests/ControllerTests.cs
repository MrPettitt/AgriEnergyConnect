using AgriEnergyConnect.Controllers;
using AgriEnergyConnect.Models;
using AgriEnergyConnect.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace AgriEnergyConnect.Tests;

public class MarketplaceTests
{
    [Fact]
    public async Task Index_FiltersBySearchAndCategory()
    {
        using var db = new TestDb();
        var farmer = db.AddFarmer("Ann", "u1");
        db.AddProduct(farmer, "Red Apple", "Fruit");
        db.AddProduct(farmer, "Green Apple", "Fruit");
        db.AddProduct(farmer, "Carrot", "Vegetable");

        var controller = new MarketplaceController(db.Context);

        var apples = (MarketplaceViewModel)((ViewResult)await controller.Index("apple", null)).Model!;
        Assert.Equal(2, apples.Results.TotalCount);

        var veg = (MarketplaceViewModel)((ViewResult)await controller.Index(null, "Vegetable")).Model!;
        Assert.Single(veg.Results.Items);
        Assert.Equal("Carrot", veg.Results.Items[0].ProductName);
    }

    [Fact]
    public async Task Details_ReturnsNotFoundForUnknownProduct()
    {
        using var db = new TestDb();
        var result = await new MarketplaceController(db.Context).Details(999);
        Assert.IsType<NotFoundResult>(result);
    }
}

public class FarmerControllerTests
{
    private static ProductFormViewModel Form(string name = "Pears") => new()
    {
        ProductName = name,
        ProductCategory = "Fruit",
        Unit = "kg",
        ProductionDate = DateTime.Today,
        Price = 3m,
    };

    private static FarmerController ControllerFor(TestDb db, string userId) =>
        TestDb.WithUser(new FarmerController(db.Context, NullLogger<FarmerController>.Instance), userId);

    [Fact]
    public async Task AddProduct_AssignsProductToSignedInFarmer()
    {
        using var db = new TestDb();
        var farmer = db.AddFarmer("Ann", "u1");

        var result = await ControllerFor(db, "u1").AddProduct(Form());

        Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(farmer.FarmerId, db.Context.Products.Single().FarmerId);
    }

    [Fact]
    public async Task EditAndDelete_CannotTouchAnotherFarmersProduct()
    {
        using var db = new TestDb();
        var ann = db.AddFarmer("Ann", "u1");
        db.AddFarmer("Bob", "u2");
        var product = db.AddProduct(ann, "Apples", "Fruit");

        var bob = ControllerFor(db, "u2");

        Assert.IsType<NotFoundResult>(await bob.EditProduct(product.ProductId, Form("Hacked")));
        Assert.IsType<NotFoundResult>(await bob.DeleteProduct(product.ProductId));
        Assert.Equal("Apples", db.Context.Products.Single().ProductName);
    }

    [Fact]
    public async Task EditProduct_UpdatesOwnProduct()
    {
        using var db = new TestDb();
        var ann = db.AddFarmer("Ann", "u1");
        var product = db.AddProduct(ann, "Apples", "Fruit");

        await ControllerFor(db, "u1").EditProduct(product.ProductId, Form("Green Apples"));

        Assert.Equal("Green Apples", db.Context.Products.Single().ProductName);
    }

    [Fact]
    public async Task DeleteProduct_RemovesOwnProduct()
    {
        using var db = new TestDb();
        var ann = db.AddFarmer("Ann", "u1");
        var product = db.AddProduct(ann, "Apples", "Fruit");

        await ControllerFor(db, "u1").DeleteProduct(product.ProductId);

        Assert.Empty(db.Context.Products);
    }

    [Fact]
    public async Task Index_ShowsOnlyOwnProducts()
    {
        using var db = new TestDb();
        var ann = db.AddFarmer("Ann", "u1");
        var bob = db.AddFarmer("Bob", "u2");
        db.AddProduct(ann, "Apples", "Fruit");
        db.AddProduct(bob, "Beans", "Vegetable");

        var model = (FarmerDashboardViewModel)((ViewResult)await ControllerFor(db, "u1").Index()).Model!;

        Assert.Equal("Apples", Assert.Single(model.Products.Items).ProductName);
    }
}

public class EmployeeProductFilterTests
{
    [Fact]
    public async Task ViewProducts_FiltersByFarmerCategoryAndDates()
    {
        using var db = new TestDb();
        var ann = db.AddFarmer("Ann", "u1");
        var bob = db.AddFarmer("Bob", "u2");
        db.AddProduct(ann, "Old Apples", "Fruit", daysAgo: 30);
        db.AddProduct(ann, "New Apples", "Fruit", daysAgo: 2);
        db.AddProduct(ann, "Carrots", "Vegetable", daysAgo: 2);
        db.AddProduct(bob, "Pears", "Fruit", daysAgo: 2);

        // The product-filtering action does not use the user manager.
        var controller = new EmployeeController(db.Context, null!, NullLogger<EmployeeController>.Instance);

        async Task<PagedResult<Product>> Run(ProductFilterViewModel filter) =>
            ((ProductFilterViewModel)((ViewResult)await controller.ViewProducts(filter)).Model!).Results;

        Assert.Equal(4, (await Run(new ProductFilterViewModel())).TotalCount);
        Assert.Equal(3, (await Run(new ProductFilterViewModel { FarmerId = ann.FarmerId })).TotalCount);
        Assert.Equal(2, (await Run(new ProductFilterViewModel { Category = "Fruit", StartDate = DateTime.Today.AddDays(-5) })).TotalCount);
        Assert.Single((await Run(new ProductFilterViewModel { FarmerId = ann.FarmerId, EndDate = DateTime.Today.AddDays(-10) })).Items);
    }
}

public class PagedResultTests
{
    [Fact]
    public async Task CreateAsync_ClampsPageAndSlices()
    {
        using var db = new TestDb();
        var farmer = db.AddFarmer("Ann", "u1");
        for (var i = 1; i <= 25; i++)
        {
            db.AddProduct(farmer, $"P{i:00}", "Fruit");
        }

        var query = db.Context.Products.OrderBy(p => p.ProductName);

        var second = await PagedResult<Product>.CreateAsync(query, 2, 10);
        Assert.Equal(10, second.Items.Count);
        Assert.Equal(3, second.TotalPages);

        var clamped = await PagedResult<Product>.CreateAsync(query, 99, 10);
        Assert.Equal(3, clamped.Page);
        Assert.Equal(5, clamped.Items.Count);
    }
}
