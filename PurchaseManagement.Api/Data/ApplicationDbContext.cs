using Microsoft.EntityFrameworkCore;
using PurchaseManagement.Api.Models;

namespace PurchaseManagement.Api.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Location> Locations { get; set; } = null!;
    public DbSet<PurchaseBill> PurchaseBills { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Location>(entity =>
        {
            entity.ToTable("Location_Details");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.LocationCode).HasColumnName("Location_Code").IsRequired().HasMaxLength(100);
            entity.Property(e => e.LocationName).HasColumnName("Location_Name").IsRequired().HasMaxLength(200);
            entity.Property(e => e.CreatedAt).HasColumnName("CreatedAt").HasDefaultValueSql("SYSUTCDATETIME()");
        });

        modelBuilder.Entity<PurchaseBill>(entity =>
        {
            entity.ToTable("Purchase_Bills");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Item).HasColumnName("Item").IsRequired().HasMaxLength(100);
            entity.Property(e => e.Batch).HasColumnName("Batch").IsRequired().HasMaxLength(200);
            entity.Property(e => e.StandardCost).HasColumnName("Standard_Cost").HasColumnType("decimal(18,2)");
            entity.Property(e => e.StandardPrice).HasColumnName("Standard_Price").HasColumnType("decimal(18,2)");
            entity.Property(e => e.Quantity).HasColumnName("Quantity");
            entity.Property(e => e.Discount).HasColumnName("Discount").HasColumnType("decimal(18,2)");
            entity.Property(e => e.TotalCost).HasColumnName("Total_Cost").HasColumnType("decimal(18,2)");
            entity.Property(e => e.TotalSelling).HasColumnName("Total_Selling").HasColumnType("decimal(18,2)");
            entity.Property(e => e.CreatedAt).HasColumnName("CreatedAt").HasDefaultValueSql("SYSUTCDATETIME()");
        });
    }
}
