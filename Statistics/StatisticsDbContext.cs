using Microsoft.EntityFrameworkCore;
using RandomizerCore;
using RandomizerCore.Overworld;
using System.Diagnostics.CodeAnalysis;

namespace Z2Randomizer.Statistics;

internal class StatisticsDbContext : DbContext
{
    public DbSet<Result> Results { get; set; }
    public DbSet<RandomizerProperties> Properties { get; set; }
    private static bool _created = false;

    private string dbPath;

    [UnconditionalSuppressMessage("Trimming", 
        "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", 
        Justification = "Statistics Doesn't ship with the trimmed package")]
    public StatisticsDbContext(string dbPath) : base()
    {
        this.dbPath = dbPath;
        if (!_created)
        {
            _created = true;
            Database.EnsureDeleted();
            Database.EnsureCreated();
        }
    }
    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlite($"Data Source={dbPath}");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
        .Entity<RandomizerProperties>()
        .Property(e => e.Climate)
        .HasConversion(
            v => v.Name,
            v => Climates.ByName(v));
    }
}