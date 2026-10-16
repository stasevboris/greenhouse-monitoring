// GreenhouseMonitoring/Data/ApplicationDbContext.cs
using GreenhouseMonitoring.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ActionEntity = GreenhouseMonitoring.Models.Action;

namespace GreenhouseMonitoring.Data;

// ✅ Объявляем класс как partial
public partial class ApplicationDbContext : IdentityDbContext<IdentityUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // === Твои сущности ===
    public DbSet<ActionEntity> Actions => Set<ActionEntity>();
    public DbSet<Crop> Crops => Set<Crop>();
    public DbSet<Event> Events => Set<Event>();
    public DbSet<Greenhouse> Greenhouses => Set<Greenhouse>();
    public DbSet<NormativeParameter> NormativeParameters => Set<NormativeParameter>();
    public DbSet<Sensor> Sensors => Set<Sensor>();
    public DbSet<SensorReading> SensorReadings => Set<SensorReading>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder); // ← Обязательно для Identity!

        // === Action ===
        modelBuilder.Entity<ActionEntity>(entity =>
        {
            entity.HasKey(e => e.ActionId).HasName("PK__Actions__FFE3F4B94DE0226A");
            entity.Property(e => e.ActionId).HasColumnName("ActionID");
            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.EventId).HasColumnName("EventID");
            entity.Property(e => e.Status).HasMaxLength(20);
            entity.Property(e => e.Type).HasMaxLength(30);

            entity.HasOne(d => d.Event)
                  .WithMany(p => p.Actions)
                  .HasForeignKey(d => d.EventId)
                  .OnDelete(DeleteBehavior.SetNull)
                  .HasConstraintName("FK_Actions_Events");
        });

        // === Crop ===
        modelBuilder.Entity<Crop>(entity =>
        {
            entity.HasKey(e => e.CropId).HasName("PK__Crops__92356135DFC122CE");
            entity.Property(e => e.CropId).HasColumnName("CropID");
            entity.Property(e => e.Name).HasMaxLength(50);
        });

        // === Event ===
        modelBuilder.Entity<Event>(entity =>
        {
            entity.HasKey(e => e.EventId).HasName("PK__Events__7944C870F7165A12");
            entity.Property(e => e.EventId).HasColumnName("EventID");
            entity.Property(e => e.Criticality).HasMaxLength(20);
            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.ReadingId).HasColumnName("ReadingID");
            entity.Property(e => e.SensorId).HasColumnName("SensorID");

            entity.HasOne(d => d.Reading)
                  .WithMany(p => p.Events)
                  .HasForeignKey(d => d.ReadingId)
                  .OnDelete(DeleteBehavior.ClientSetNull)
                  .HasConstraintName("FK_Events_Readings");

            entity.HasOne(d => d.Sensor)
                  .WithMany(p => p.Events)
                  .HasForeignKey(d => d.SensorId)
                  .HasConstraintName("FK_Events_Sensors");
        });

        // === Greenhouse ===
        modelBuilder.Entity<Greenhouse>(entity =>
        {
            entity.HasKey(e => e.GreenhouseId).HasName("PK__Greenhou__E551F0E889FD018B");
            entity.Property(e => e.GreenhouseId).HasColumnName("GreenhouseID");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.CropId).HasColumnName("CropID");
            entity.Property(e => e.Location).HasMaxLength(100);
            entity.Property(e => e.NameNumber).HasMaxLength(50);

            entity.HasOne(d => d.Crop)
                  .WithMany(p => p.Greenhouses)
                  .HasForeignKey(d => d.CropId)
                  .HasConstraintName("FK_Greenhouses_Crops");
        });

        // === NormativeParameter ===
        modelBuilder.Entity<NormativeParameter>(entity =>
        {
            entity.HasKey(e => e.ParameterId).HasName("PK__Normativ__F80C6297413D8948");
            entity.Property(e => e.ParameterId).HasColumnName("ParameterID");
            entity.Property(e => e.CropId).HasColumnName("CropID");
            entity.Property(e => e.GrowthStage).HasMaxLength(50);
            entity.Property(e => e.MaxValue).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.MinValue).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Parameter).HasMaxLength(50);

            entity.HasOne(d => d.Crop)
                  .WithMany(p => p.NormativeParameters)
                  .HasForeignKey(d => d.CropId)
                  .IsRequired() // ← 🔑 ДОБАВЬТЕ ЭТУ СТРОКУ
                  .HasConstraintName("FK_NormativeParameters_Crops");
        });

        // === Sensor ===
        modelBuilder.Entity<Sensor>(entity =>
        {
            entity.HasKey(e => e.SensorId).HasName("PK__Sensors__D809841AA90797F2");
            entity.Property(e => e.SensorId).HasColumnName("SensorID");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.GreenhouseId).HasColumnName("GreenhouseID");
            entity.Property(e => e.LocationInGh).HasMaxLength(100).HasColumnName("LocationInGH");
            entity.Property(e => e.Model).HasMaxLength(50);
            entity.Property(e => e.Type).HasMaxLength(30);

            entity.HasOne(d => d.Greenhouse)
                  .WithMany(p => p.Sensors)
                  .HasForeignKey(d => d.GreenhouseId)
                  .HasConstraintName("FK_Sensors_Greenhouses");
        });

        // === SensorReading ===
        modelBuilder.Entity<SensorReading>(entity =>
        {
            entity.HasKey(e => e.ReadingId).HasName("PK__SensorRe__C80F9C6ECB2DF7B0");
            entity.Property(e => e.ReadingId).HasColumnName("ReadingID");
            entity.Property(e => e.SensorId).HasColumnName("SensorID");
            entity.Property(e => e.Value).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.Sensor)
                  .WithMany(p => p.SensorReadings)
                  .HasForeignKey(d => d.SensorId)
                  .HasConstraintName("FK_SensorReadings_Sensors");
        });

        // ✅ Вызов частичного метода (безопасен даже если реализация отсутствует)
        OnModelCreatingPartial(modelBuilder);
    }

    // ✅ Объявление partial-метода (не класса!)
    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}