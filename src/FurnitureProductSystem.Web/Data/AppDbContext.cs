using FurnitureProductSystem.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace FurnitureProductSystem.Web.Data;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductType> ProductTypes => Set<ProductType>();
    public DbSet<MaterialType> MaterialTypes => Set<MaterialType>();
    public DbSet<Workshop> Workshops => Set<Workshop>();
    public DbSet<ProductWorkshop> ProductWorkshops => Set<ProductWorkshop>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Product>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Article).HasMaxLength(64).IsRequired();
            e.Property(x => x.Name).HasMaxLength(200).IsRequired();
            e.Property(x => x.MinPartnerCost).HasPrecision(18, 2);

            e.HasOne(x => x.ProductType)
                .WithMany(t => t.Products)
                .HasForeignKey(x => x.ProductTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.MaterialType)
                .WithMany(m => m.Products)
                .HasForeignKey(x => x.MaterialTypeId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ProductType>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).HasMaxLength(150).IsRequired();
            e.Property(x => x.Coefficient).HasPrecision(18, 6);
        });

        modelBuilder.Entity<MaterialType>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).HasMaxLength(150).IsRequired();
            e.Property(x => x.LossPercent).HasPrecision(18, 6);
        });

        modelBuilder.Entity<Workshop>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).HasMaxLength(150).IsRequired();
        });

        modelBuilder.Entity<ProductWorkshop>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.ProductId, x.WorkshopId }).IsUnique(false);

            e.HasOne(x => x.Product)
                .WithMany(p => p.Workshops)
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.Workshop)
                .WithMany(w => w.Products)
                .HasForeignKey(x => x.WorkshopId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
