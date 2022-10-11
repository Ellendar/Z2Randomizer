using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Z2Randomizer
{
    internal class StatisticsDbContext : DbContext
    {
        public DbSet<Result> Results { get; set; }
        private static bool _created = false;

        private string dbPath;

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
    }
}
