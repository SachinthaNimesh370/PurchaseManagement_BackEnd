using Microsoft.EntityFrameworkCore;
using PurchaseManagement.Api.Models;

namespace PurchaseManagement.Api.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Location> Locations { get; set; } = null!;

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
    }
}
