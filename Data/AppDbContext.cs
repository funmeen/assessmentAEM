using Microsoft.EntityFrameworkCore;
using PlatformWellSync.Models;

namespace PlatformWellSync.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Platform> Platforms { get; set; }
    public DbSet<Well> Wells { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Platform>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever(); // Use API-provided Id
            entity.Property(e => e.PlatformName).HasMaxLength(200);
            entity.HasMany(e => e.Wells)
                  .WithOne(w => w.Platform)
                  .HasForeignKey(w => w.PlatformId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Well>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever(); // Use API-provided Id
            entity.Property(e => e.UniqueName).HasMaxLength(200);
        });
    }
}
