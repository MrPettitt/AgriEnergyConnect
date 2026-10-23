using System.Security.Claims;
using AgriEnergyConnect.Data;
using AgriEnergyConnect.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace AgriEnergyConnect.Tests;

/// <summary>An isolated in-memory SQLite database with the real schema, one per test.</summary>
public sealed class TestDb : IDisposable
{
    private readonly SqliteConnection _connection = new("DataSource=:memory:");

    public TestDb()
    {
        _connection.Open();
        Context = new ApplicationDbContext(
            new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlite(_connection).Options);
        Context.Database.EnsureCreated();
    }

    public ApplicationDbContext Context { get; }

    public Farmer AddFarmer(string name, string userId)
    {
        Context.Users.Add(new IdentityUser
        {
            Id = userId,
            UserName = $"{userId}@test.local",
            Email = $"{userId}@test.local",
        });
        var farmer = new Farmer { FullName = name, ContactNumber = "0123456789", Address = "1 Test Rd", UserId = userId };
        Context.Farmers.Add(farmer);
        Context.SaveChanges();
        return farmer;
    }

    public Product AddProduct(Farmer farmer, string name, string category, int daysAgo = 1, decimal price = 1m)
    {
        var product = new Product
        {
            FarmerId = farmer.FarmerId,
            ProductName = name,
            ProductCategory = category,
            ProductionDate = DateTime.Today.AddDays(-daysAgo),
            Price = price,
        };
        Context.Products.Add(product);
        Context.SaveChanges();
        return product;
    }

    public static T WithUser<T>(T controller, string userId) where T : Controller
    {
        var http = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, userId) }, "test")),
        };
        controller.ControllerContext = new ControllerContext { HttpContext = http };
        controller.TempData = new TempDataDictionary(http, new NullTempDataProvider());
        return controller;
    }

    public void Dispose()
    {
        Context.Dispose();
        _connection.Dispose();
    }

    private sealed class NullTempDataProvider : ITempDataProvider
    {
        public IDictionary<string, object> LoadTempData(HttpContext context) => new Dictionary<string, object>();

        public void SaveTempData(HttpContext context, IDictionary<string, object> values)
        {
        }
    }
}
