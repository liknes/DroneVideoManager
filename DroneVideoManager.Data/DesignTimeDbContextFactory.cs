using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DroneVideoManager.Data
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<DroneVideoDbContext>
    {
        public DroneVideoDbContext CreateDbContext(string[] args)
        {
            var dbPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "DroneVideoManager",
                "dronevideo.db");

            var optionsBuilder = new DbContextOptionsBuilder<DroneVideoDbContext>();
            optionsBuilder.UseSqlite($"Data Source={dbPath}");

            return new DroneVideoDbContext(optionsBuilder.Options);
        }
    }
} 