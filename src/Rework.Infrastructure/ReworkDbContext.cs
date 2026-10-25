using Microsoft.EntityFrameworkCore;
using Rework.Application;
using Rework.Domain.Entities;

namespace Rework.Infrastructure;

public class ReworkDbContext(DbContextOptions<ReworkDbContext> options) : DbContext(options)
{
    public DbSet<ReworkItem> ReworkItems => Set<ReworkItem>();
    public DbSet<AppSetting> AppSettings => Set<AppSetting>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<ReworkItem>(e =>
        {
            e.ToTable("ReworkItems");
            e.HasKey(x => x.Id);
            e.Property(x => x.DefectDescription).HasMaxLength(1000).IsRequired();
            e.Property(x => x.StationId).HasMaxLength(20).IsRequired();
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.AssignedTo).HasMaxLength(100);
            e.Property(x => x.RowVersion).IsRowVersion();
            e.HasIndex(x => new { x.Status, x.CreatedAt });
            e.HasIndex(x => x.StationId);
        });

        b.Entity<AppSetting>(e =>
        {
            e.ToTable("AppSettings");
            e.HasKey(x => x.Key);
            e.Property(x => x.Key).HasMaxLength(100);
            e.Property(x => x.Value).HasMaxLength(2000).IsRequired();
            e.Property(x => x.Category).HasMaxLength(50).IsRequired();
            e.Property(x => x.RowVersion).IsRowVersion();
            e.HasIndex(x => x.Category);
        });
    }
}
