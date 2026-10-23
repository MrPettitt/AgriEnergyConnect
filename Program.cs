using AgriEnergyConnect.Data;
using Microsoft.AspNetCore.Identity;
using System.Globalization;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// SQLite keeps the project self-contained: no SQL Server / LocalDB install required.
// The database file is created by migrations on first run. Demo data is only seeded
// when "Seed:DemoData" is true (Development by default).
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' was not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));

builder.Services.AddDefaultIdentity<IdentityUser>(options =>
    {
        // No email sender is configured, so requiring confirmed accounts would leave
        // self-registered users permanently unable to sign in. Demo accounts created
        // via seeding or the "Add Farmer" workflow are already marked as confirmed.
        options.SignIn.RequireConfirmedAccount = false;

        options.Password.RequiredLength = 8;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddControllersWithViews();

builder.Services.AddRazorPages();

builder.Services.AddResponseCompression(options => options.EnableForHttps = true);

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseResponseCompression();

// Use one fixed culture so prices and dates look the same on every server.
// Change "App:Culture" in appsettings.json (for example to "en-ZA") to localise.
var culture = new CultureInfo(app.Configuration["App:Culture"] ?? "en-US");
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(culture),
    SupportedCultures = new[] { culture },
    SupportedUICultures = new[] { culture },
    RequestCultureProviders = new List<IRequestCultureProvider>(),
});

app.UseStaticFiles(new StaticFileOptions
{
    // Static files are fingerprinted with asp-append-version, so they can be cached for a long time.
    OnPrepareResponse = ctx => ctx.Context.Response.Headers.CacheControl = "public,max-age=604800",
});

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Farmers are onboarded by employees, so the built-in public registration page is
// hidden from everyone else. (The page is marked [AllowAnonymous], so a route
// convention cannot restrict it.)
app.Use(async (context, next) =>
{
    var isRegisterPage = context.Request.Path.StartsWithSegments("/Identity/Account/Register", StringComparison.OrdinalIgnoreCase);
    if (isRegisterPage && !context.User.IsInRole("Employee"))
    {
        context.Response.StatusCode = StatusCodes.Status404NotFound;
        return;
    }

    await next();
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

using (var scope = app.Services.CreateScope())
{
    await SeedData.InitializeAsync(scope.ServiceProvider, app.Configuration.GetValue<bool>("Seed:DemoData"));
}

app.Run();

// Exposed so integration tests can use WebApplicationFactory<Program>.
public partial class Program { }
