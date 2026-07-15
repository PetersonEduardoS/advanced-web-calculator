using AdvancedCalculator.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace AdvancedCalculator.Web.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<CalculationHistory> CalculationHistories => Set<CalculationHistory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<CalculationHistory>(entity =>
        {
            entity.HasIndex(e => e.CreatedAtUtc);
        });
    }
}