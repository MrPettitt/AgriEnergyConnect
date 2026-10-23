using AgriEnergyConnect.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AgriEnergyConnect.Data;

/// <summary>
/// Applies pending migrations and ensures the "Farmer" and "Employee" roles exist.
/// When demo seeding is enabled (configuration key "Seed:DemoData", on by default in
/// Development only) it also creates sample employees, farmers and products so the
/// app is immediately explorable after a fresh clone.
/// </summary>
public static class SeedData
{
    private const string EmployeePassword = "Employee123!";
    private const string FarmerPassword = "Farmer123!";

    private sealed record EmployeeSeed(string Email, string FullName, string Contact, string Position);

    private sealed record ProductSeed(string Name, string Category, int DaysAgo, decimal Price);

    private sealed record FarmerSeed(string Email, string FullName, string Contact, string Address, ProductSeed[] Products);

    private static readonly EmployeeSeed[] Employees =
    {
        new("john.smith@demo.com", "John Smith", "0987654321", "Farm Manager"),
        new("jane.doe@demo.com", "Jane Doe", "9876543210", "Agricultural Specialist"),
        new("william.jones@demo.com", "William Jones", "0987123456", "Field Supervisor"),
        new("emily.davis@demo.com", "Emily Davis", "0987234567", "Crop Consultant"),
        new("michael.wilson@demo.com", "Michael Wilson", "0987345678", "Logistics Coordinator"),
    };

    private static readonly FarmerSeed[] Farmers =
    {
        new("john.doe@demo.com", "John Doe", "1234567891", "123 Main St, Springfield, IL", new ProductSeed[]
        {
            new("Apple", "Fruit", 10, 2.50m), new("Carrot", "Vegetable", 6, 1.20m), new("Banana", "Fruit", 12, 1.80m), new("Potato", "Vegetable", 2, 0.90m),
        }),
        new("jane.smith@demo.com", "Jane Smith", "1234567892", "456 Elm St, Shelbyville, IL", new ProductSeed[]
        {
            new("Tomato", "Vegetable", 8, 2.10m), new("Lettuce", "Vegetable", 5, 1.50m), new("Strawberry", "Fruit", 15, 4.20m), new("Onion", "Vegetable", 3, 1.00m),
        }),
        new("michael.brown@demo.com", "Michael Brown", "1234567893", "789 Maple Ave, Capital City, IL", new ProductSeed[]
        {
            new("Grapes", "Fruit", 9, 3.60m), new("Cabbage", "Vegetable", 7, 1.30m), new("Peach", "Fruit", 11, 2.90m), new("Radish", "Vegetable", 1, 1.10m),
        }),
        new("emily.johnson@demo.com", "Emily Johnson", "1234567894", "321 Oak St, Ogdenville, IL", new ProductSeed[]
        {
            new("Orange", "Fruit", 14, 2.30m), new("Broccoli", "Vegetable", 4, 2.00m), new("Blueberry", "Fruit", 13, 5.50m), new("Garlic", "Vegetable", 5, 3.00m),
        }),
        new("william.white@demo.com", "William White", "1234567895", "654 Pine St, North Haverbrook, IL", new ProductSeed[]
        {
            new("Pear", "Fruit", 10, 2.40m), new("Spinach", "Vegetable", 6, 1.90m), new("Plum", "Fruit", 12, 2.70m), new("Pepper", "Vegetable", 2, 1.60m),
        }),
    };

    public static async Task InitializeAsync(IServiceProvider services, bool seedDemoData)
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        await context.Database.MigrateAsync();

        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<IdentityUser>>();

        await EnsureRolesAsync(roleManager);

        if (!seedDemoData)
        {
            return;
        }

        await EnsureEmployeesAsync(userManager, context);
        await EnsureFarmersAsync(userManager, context);
    }

    private static async Task EnsureRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        foreach (var roleName in new[] { "Farmer", "Employee" })
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }
    }

    private static async Task EnsureEmployeesAsync(UserManager<IdentityUser> userManager, ApplicationDbContext context)
    {
        foreach (var seed in Employees)
        {
            if (await userManager.FindByEmailAsync(seed.Email) is not null)
            {
                continue;
            }

            var user = new IdentityUser { UserName = seed.Email, Email = seed.Email, EmailConfirmed = true };
            var result = await userManager.CreateAsync(user, EmployeePassword);
            if (!result.Succeeded)
            {
                continue;
            }

            await userManager.AddToRoleAsync(user, "Employee");

            context.Employees.Add(new Employee
            {
                FullName = seed.FullName,
                ContactNumber = seed.Contact,
                Position = seed.Position,
                UserId = user.Id,
            });

            await context.SaveChangesAsync();
        }
    }

    private static async Task EnsureFarmersAsync(UserManager<IdentityUser> userManager, ApplicationDbContext context)
    {
        foreach (var seed in Farmers)
        {
            if (await userManager.FindByEmailAsync(seed.Email) is not null)
            {
                continue;
            }

            var user = new IdentityUser { UserName = seed.Email, Email = seed.Email, EmailConfirmed = true };
            var result = await userManager.CreateAsync(user, FarmerPassword);
            if (!result.Succeeded)
            {
                continue;
            }

            await userManager.AddToRoleAsync(user, "Farmer");

            var farmer = new Farmer
            {
                FullName = seed.FullName,
                ContactNumber = seed.Contact,
                Address = seed.Address,
                UserId = user.Id,
            };
            context.Farmers.Add(farmer);
            await context.SaveChangesAsync();

            foreach (var product in seed.Products)
            {
                context.Products.Add(new Product
                {
                    FarmerId = farmer.FarmerId,
                    ProductName = product.Name,
                    ProductCategory = product.Category,
                    Unit = "kg",
                    ProductionDate = DateTime.Today.AddDays(-product.DaysAgo),
                    Price = product.Price,
                });
            }

            await context.SaveChangesAsync();
        }
    }
}
