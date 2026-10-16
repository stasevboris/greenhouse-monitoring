using GreenhouseMonitoring.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GreenhouseMonitoring.Data;

public partial class AppDbContext : IdentityDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Crop> Crops { get; set; }
    public DbSet<Greenhouse> Greenhouses { get; set; }
    public DbSet<Sensor> Sensors { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Для соответствия SQL-скрипту оставляем имена без явного указания
        // EF Core автоматически использует имена свойств: CropId, GreenhouseId, SensorId

        modelBuilder.Entity<Crop>(entity =>
        {
            entity.Property(e => e.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<Greenhouse>(entity =>
        {
            entity.Property(e => e.NameNumber).HasMaxLength(100);
            entity.Property(e => e.Location).HasMaxLength(200);
            entity.Property(e => e.CreatedAt)
                  .HasDefaultValueSql("GETDATE()");

            entity.HasOne(g => g.Crop)
                  .WithMany(c => c.Greenhouses)
                  .HasForeignKey(g => g.CropId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Sensor>(entity =>
        {
            entity.Property(e => e.LocationInGh).HasMaxLength(100);
            entity.Property(e => e.Model).HasMaxLength(100);
            entity.Property(e => e.Type).HasMaxLength(50);
            entity.Property(e => e.CreatedAt)
                  .HasDefaultValueSql("GETDATE()");

            entity.HasOne(s => s.Greenhouse)
                  .WithMany(g => g.Sensors)
                  .HasForeignKey(s => s.GreenhouseId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}