using AgriEnergyConnect.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AgriEnergyConnect.Data;

public class ApplicationDbContext : IdentityDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Farmer> Farmers => Set<Farmer>();

    public DbSet<Product> Products => Set<Product>();

    public DbSet<Employee> Employees => Set<Employee>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Employee>()
            .HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId);

        modelBuilder.Entity<Farmer>()
            .HasOne(f => f.User)
            .WithMany()
            .HasForeignKey(f => f.UserId);

        modelBuilder.Entity<Product>()
            .HasOne(p => p.Farmer)
            .WithMany(f => f.Products)
            .HasForeignKey(p => p.FarmerId);

        modelBuilder.Entity<Product>(entity =>
        {
            entity.Property(p => p.ProductName).HasMaxLength(100).IsRequired();
            entity.Property(p => p.ProductCategory).HasMaxLength(100).IsRequired();
            entity.Property(p => p.Unit).HasMaxLength(20).IsRequired();
            entity.Property(p => p.Price).HasPrecision(10, 2);
            entity.HasIndex(p => p.ProductCategory);
            entity.HasIndex(p => p.ProductionDate);
        });
    }
}
